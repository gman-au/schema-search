using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoFixture;
using AutoFixture.AutoNSubstitute;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using SchemaSearch.Application;
using SchemaSearch.Domain.Schema;
using SchemaSearch.Interfaces;
using Xunit;

namespace SchemaSearch.Tests.Unit
{
    public class SchemaSearchApplicationTests
    {
        private readonly TestContext _context = new();

        [Fact]
        public async Task Test_Schema_Extraction_Normal()
        {
            _context.ArrangeApplicationWithDefaults();
            await _context.ActPerformExtraction();
            _context.AssertResults();
        }

        [Fact]
        public async Task Test_Schema_Extraction_Null_Logger()
        {
            _context.ArrangeApplicationWithNoLogger();
            await _context.ActPerformExtraction();
            _context.AssertResults();
        }

        private class TestContext
        {
            private readonly IFixture _fixture;
            private readonly ISchemaExtractor _schemaExtractor;
            private readonly SchemaSearchApplication _sut;
            private ILogger<SchemaSearchApplication> _logger;
            private IEnumerable<SchemaTable> _results;

            public TestContext()
            {
                _fixture = new Fixture().Customize(new AutoNSubstituteCustomization());

                _fixture.Behaviors.OfType<ThrowingRecursionBehavior>()
                    .ToList()
                    .ForEach(b => _fixture.Behaviors.Remove(b));

                _fixture.Behaviors.Add(new OmitOnRecursionBehavior());

                _schemaExtractor = Substitute.For<ISchemaExtractor>();
                _logger = Substitute.For<ILogger<SchemaSearchApplication>>();

                _sut =
                    new SchemaSearchApplication(
                        _schemaExtractor,
                        _logger
                    );
            }

            public void ArrangeApplicationWithDefaults()
            {
                _schemaExtractor
                    .PerformAsync()
                    .ReturnsForAnyArgs(
                        _fixture
                            .CreateMany<SchemaTable>(5)
                    );
            }

            public void ArrangeApplicationWithNoLogger()
            {
                _logger = NullLogger<SchemaSearchApplication>.Instance;
                _schemaExtractor
                    .PerformAsync()
                    .ReturnsForAnyArgs(
                        _fixture
                            .CreateMany<SchemaTable>(5)
                    );
            }

            public async Task ActPerformExtraction()
            {
                _results =
                    await
                        _sut
                            .RunAsync();
            }

            public void AssertResults()
            {
                Assert.Equal(5, _results.Count());
            }
        }
    }
}