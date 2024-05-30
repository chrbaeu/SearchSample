using FluentAssertions;
using SearchSample.RequestModels;
using SearchSampleLuceneProvider;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SearchSampleTests;

public class LuceneSearchServiceTests
{
    private readonly LuceneSearchService sut = new();

    [Fact]
    public void Add_ShouldAddDocument_ToEmptyIndex()
    {
        // Arrange
        var documentToAdd = CreateSampleDocuments().First();

        // Act
        sut.Add(documentToAdd);

        // Assert
        sut.Exists(documentToAdd.Uuid);
        sut.GetAllDocuments().Should().ContainEquivalentOf(documentToAdd);
    }

    [Fact]
    public void Add_ShouldAddDocument_ToNonEmptyIndex()
    {
        // Arrange
        var existingDocument = CreateSampleDocuments().First();
        sut.Add(existingDocument);
        var documentToAdd = CreateSampleDocuments().Last();

        // Act
        sut.Add(documentToAdd);

        // Assert
        sut.Exists(documentToAdd.Uuid);
        sut.GetAllDocuments().Should().ContainEquivalentOf(documentToAdd);
    }

    [Fact]
    public void Add_MultipleDocuments_ShouldAddAllDocuments()
    {
        // Arrange
        var documents = CreateSampleDocuments();

        // Act
        sut.Add(documents);

        // Assert
        sut.GetAllDocuments().Should().BeEquivalentTo(documents);
    }

    [Fact]
    public void Exists_ShouldReturnTrue_ForExistingDocument()
    {
        // Arrange
        var document = CreateSampleDocuments().First();
        sut.Add(document);

        // Act
        var exists = sut.Exists(document.Uuid);

        // Assert
        exists.Should().BeTrue();
    }

    [Fact]
    public void Exists_ShouldReturnFalse_ForEmptyIndex()
    {
        // Arrange
        var nonExistingUuid = Guid.NewGuid();

        // Act
        var exists = sut.Exists(nonExistingUuid);

        // Assert
        exists.Should().BeFalse();
    }

    [Fact]
    public void Exists_ShouldReturnFalse_ForNonExistingDocument()
    {
        // Arrange
        var documents = CreateSampleDocuments();
        sut.Add(documents);
        var nonExistingUuid = Guid.NewGuid();

        // Act
        var exists = sut.Exists(nonExistingUuid);

        // Assert
        exists.Should().BeFalse();
    }

    [Fact]
    public void Update_ShouldModifyExistingDocument()
    {
        // Arrange
        var document = CreateSampleDocuments().First();
        sut.Add(document);
        var updatedDocument = new SearchableDocument
        {
            Uuid = document.Uuid,
            Title = "Updated Title",
            FullText = "Updated FullText",
            FilterableItems = []
        };

        // Act
        sut.Update(updatedDocument);

        // Assert
        var retrievedDocuments = sut.FindDocuments(new SearchRequest { SearchQuery = "Updated Title" });
        retrievedDocuments.Should().HaveCount(1);
        retrievedDocuments.Should().ContainEquivalentOf(updatedDocument);
    }

    [Fact]
    public void Update_MultipleDocuments_ShouldModifyExistingDocuments()
    {
        // Arrange
        var documents = CreateSampleDocuments();
        sut.Add(documents);
        var updatedDocuments = documents.Select(doc => new SearchableDocument
        {
            Uuid = doc.Uuid,
            Title = doc.Title + " Updated",
            FullText = doc.FullText + " Updated",
            FilterableItems = doc.FilterableItems
        }).ToList();

        // Act
        sut.Update(updatedDocuments);

        // Assert
        sut.GetAllDocuments().Should().BeEquivalentTo(updatedDocuments);
    }

    [Fact]
    public void Update_ShouldAlsoAddNonExistingDocument()
    {
        // Arrange
        var documents = CreateSampleDocuments();
        sut.Add(documents);
        var nonExistingDocument = new SearchableDocument
        {
            Uuid = Guid.NewGuid(),
            Title = "Non-Existing Title",
            FullText = "Non-Existing FullText",
            FilterableItems = []
        };

        // Act
        sut.Update(nonExistingDocument);

        // Assert
        sut.Exists(nonExistingDocument.Uuid).Should().BeTrue();
    }

    [Fact]
    public void Delete_ShouldRemoveExistingDocument()
    {
        // Arrange
        var documents = CreateSampleDocuments();
        sut.Add(documents);
        var uuid = documents.Skip(1).First().Uuid;

        // Act
        sut.Delete(uuid);

        // Assert
        sut.Exists(uuid).Should().BeFalse();
        sut.GetAllKeys().Should().NotContain(uuid);
        sut.GetAllKeys().Should().NotBeEmpty();
    }

    [Fact]
    public void Delete_MultipleDocuments_ShouldRemoveAllDocuments()
    {
        // Arrange
        var documents = CreateSampleDocuments();
        sut.Add(documents);
        var uuids = documents.Select(d => d.Uuid).ToList();

        // Act
        sut.Delete(uuids);

        // Assert
        sut.GetAllKeys().Should().BeEmpty();
    }

    [Fact]
    public void FindKeys_WithMatchingSearchQuery_ShouldReturnCorrectUuids()
    {
        // Arrange
        var documents = CreateSampleDocuments();
        sut.Add(documents);
        var searchRequest = new SearchRequest { SearchQuery = "Group1" };

        // Act
        var result = sut.FindKeys(searchRequest);

        // Assert
        result.Should().BeEquivalentTo(documents.Take(2).Select(d => d.Uuid));
    }

    [Fact]
    public void FindKeys_WithMatchingFilters_ShouldReturnCorrectUuids()
    {
        // Arrange
        var documents = CreateSampleDocuments();
        sut.Add(documents);
        var searchRequest = new SearchRequest
        {
            SearchFilters = [new SearchFilter("FilterGroup", "1")]
        };

        // Act
        var result = sut.FindKeys(searchRequest);

        // Assert
        result.Should().BeEquivalentTo(documents.Take(2).Select(d => d.Uuid));
    }

    [Fact]
    public void FindKeys_WithMatchingSearchQueryAndFilters_ShouldReturnCorrectUuids()
    {
        // Arrange
        var documents = CreateSampleDocuments();
        sut.Add(documents);
        var searchRequest = new SearchRequest
        {
            SearchQuery = "Second",
            SearchFilters = [new SearchFilter("FilterGroup", "1")]
        };

        // Act
        var result = sut.FindKeys(searchRequest);

        // Assert
        result.Should().BeEquivalentTo(documents.Skip(1).Take(1).Select(d => d.Uuid));
    }

    [Fact]
    public void FindKeys_WithNotMatchingSearchQuery_ShouldReturnEmptyList()
    {
        // Arrange
        var searchRequest = new SearchRequest { SearchQuery = "NonExisting" };

        // Act
        var result = sut.FindKeys(searchRequest);

        // Assert
        result.Should().BeEmpty();
    }


    [Fact]
    public void FindDocuments_WithMatchingSearchQuery_ShouldReturnCorrectDocuments()
    {
        // Arrange
        var documents = CreateSampleDocuments();
        sut.Add(documents);

        var searchRequest = new SearchRequest { SearchQuery = "Group1" };

        // Act
        var result = sut.FindDocuments(searchRequest);

        // Assert
        result.Should().BeEquivalentTo(documents.Take(2));
    }

    [Fact]
    public void FindSDocuments_WithMatchingFilters_ShouldReturnCorrectDocuments()
    {
        // Arrange
        var documents = CreateSampleDocuments();
        sut.Add(documents);
        var searchRequest = new SearchRequest
        {
            SearchFilters = [new SearchFilter("FilterGroup", "1")]
        };

        // Act
        var result = sut.FindDocuments(searchRequest);

        // Assert
        result.Should().BeEquivalentTo(documents.Take(2));
    }

    [Fact]
    public void FindDocuments_WithMatchingSearchQueryAndFilters_ShouldReturnCorrectDocuments()
    {
        // Arrange
        var documents = CreateSampleDocuments();
        sut.Add(documents);
        var searchRequest = new SearchRequest
        {
            SearchQuery = "Second",
            SearchFilters = [new SearchFilter("FilterGroup", "1")]
        };

        // Act
        var result = sut.FindDocuments(searchRequest);

        // Assert
        result.Should().BeEquivalentTo(documents.Skip(1).Take(1));
    }

    [Fact]
    public void FindDocuments_WithNotMatchingSearchQuery_ShouldReturnEmptyList()
    {
        // Arrange
        var searchRequest = new SearchRequest { SearchQuery = "NonExisting" };

        // Act
        var result = sut.FindDocuments(searchRequest);

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public void FindDocuments_WithNoQuery_ShouldReturnEmptyList()
    {
        // Arrange
        var searchRequest = new SearchRequest();

        // Act
        var result = sut.FindDocuments(searchRequest);

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public void FindDocuments_WithInvalidQuery_ShouldReturnEmptyList()
    {
        // Arrange
        var searchRequest = new SearchRequest() { SearchQuery = null!, SearchFilters = null! };

        // Act
        var result = sut.FindDocuments(searchRequest);

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public void FindDocuments_WithInvalidEmptyFilters_ShouldReturnEmptyList()
    {
        // Arrange
        var searchRequest = new SearchRequest()
        {
            SearchQuery = "",
            SearchFilters = [
                new SearchFilter("test", []),
                new SearchFilter("test", (List<string>?)null!),
                new SearchFilter(null!, (List<string>?)null!)
            ]
        };

        // Act
        var result = sut.FindDocuments(searchRequest);

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public void FindDocuments_WithComplexQuery_ShouldReturnCorrectDocuments()
    {
        // Arrange
        var documents = CreateSampleDocuments();
        sut.Add(documents);
        var searchRequest = new SearchRequest("('Title' | ~FullText~) & !Power")
        {
            SearchFilters = [new("FilterGroup", "1")]
        };

        // Act
        var result = sut.FindDocuments(searchRequest);

        // Assert
        result.Should().BeEquivalentTo(documents.Skip(1).Take(1));
    }

    [Fact]
    public void GetAllKeys_ShouldReturnAllDocumentUuids()
    {
        // Arrange
        var documents = CreateSampleDocuments();
        sut.Add(documents);

        // Act
        var result = sut.GetAllKeys();

        // Assert
        result.Should().BeEquivalentTo(documents.Select(d => d.Uuid));
    }

    [Fact]
    public void GetAllDocuments_ShouldReturnAllDocuments()
    {
        // Arrange
        var documents = CreateSampleDocuments();
        sut.Add(documents);

        // Act
        var result = sut.GetAllDocuments();

        // Assert
        result.Should().BeEquivalentTo(documents);
    }

    private static List<SearchableDocument> CreateSampleDocuments()
    {
        return [
            new SearchableDocument
            {
                Uuid = Guid.NewGuid(),
                Title = "First Title Power",
                FullText = "First FullText Group1",
                FilterableItems = [
                    new FilterableItem() { Category = "First Category", Value = "First Value" },
                    new FilterableItem() { Category = "FilterGroup", Value = "1" }
                ]
            },
            new SearchableDocument
            {
                Uuid = Guid.NewGuid(),
                Title = "Second Title",
                FullText = "Second FullText Group1",
                FilterableItems =  [
                    new FilterableItem() { Category = "Second Category", Value = "Second Value" },
                    new FilterableItem() { Category = "FilterGroup", Value = "1" }
                ]
            },
            new SearchableDocument
            {
                Uuid = Guid.NewGuid(),
                Title = "Third Title",
                FullText = "Third FullText Group2",
                FilterableItems =  [
                    new FilterableItem() { Category = "Third Category", Value = "Third Value" },
                    new FilterableItem() { Category = "FilterGroup", Value = "2" }
                ]
            }
        ];
    }

}
