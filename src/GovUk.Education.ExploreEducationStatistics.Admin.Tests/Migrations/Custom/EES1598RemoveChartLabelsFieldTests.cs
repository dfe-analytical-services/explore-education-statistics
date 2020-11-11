using System;
using System.Collections.Generic;
using System.Linq;
using GovUk.Education.ExploreEducationStatistics.Admin.Migrations.Custom;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Chart;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using Xunit;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Migrations.Custom
{
    public class EES1598RemoveChartLabelsFieldTests
    {
        [Fact]
        public void AddsNewDataSet()
        {
            var indicatorId = Guid.NewGuid();
            var filter1Id = Guid.NewGuid();
            var filter2Id = Guid.NewGuid();

            var dataBlock = new DataBlock
            {
                Charts = new List<IChart>
                {
                    new LineChart
                    {
                        Labels = new Dictionary<string, ChartLabelConfiguration>
                        {
                            {
                                $"{indicatorId}_{filter1Id}_{filter2Id}_____",
                                new ChartLabelConfiguration
                                {
                                    Colour = "Test colour",
                                    Label = "Test label",
                                    Symbol = ChartLineSymbol.diamond,
                                    LineStyle = ChartLineStyle.dotted
                                }
                            }
                        },
                        Axes = new Dictionary<string, ChartAxisConfiguration>
                        {
                            {
                                "major",
                                new ChartAxisConfiguration
                                {
                                    DataSets = new List<ChartDataSet>()
                                }
                            }
                        }
                    }
                },
            };

            var contextId = Guid.NewGuid().ToString();

            using (var context = ContentDbUtils.InMemoryContentDbContext(contextId))
            {
                context.Add(dataBlock);
                context.SaveChanges();
            }

            using (var context = ContentDbUtils.InMemoryContentDbContext(contextId))
            {
                var migration = new EES1598RemoveChartLabelsField(context, isTest: true);
                migration.Apply();
            }

            using (var context = ContentDbUtils.InMemoryContentDbContext(contextId))
            {
                var savedDataBlock = context.DataBlocks.Find(dataBlock.Id);

                Assert.Null(savedDataBlock.Charts[0].Labels);

                var dataSets = savedDataBlock.Charts[0].Axes?["major"].DataSets;

                Assert.NotNull(dataSets);
                Assert.Single(dataSets);

                Assert.Equal(indicatorId,  dataSets[0].Indicator);
                Assert.Equal(new List<Guid>{ filter1Id, filter2Id },  dataSets[0].Filters);

                Assert.Equal("Test colour", dataSets[0].Config.Colour);
                Assert.Equal("Test label", dataSets[0].Config.Label);
                Assert.Equal(ChartLineSymbol.diamond, dataSets[0].Config.Symbol);
                Assert.Equal(ChartLineStyle.dotted, dataSets[0].Config.LineStyle);
            }
        }

        [Fact]
        public void UpdatesExistingDataSetWithNoConfig()
        {
            var indicatorId = Guid.NewGuid();
            var filter1Id = Guid.NewGuid();
            var filter2Id = Guid.NewGuid();

            var dataBlock = new DataBlock
            {
                Charts = new List<IChart>
                {
                    new LineChart
                    {
                        Labels = new Dictionary<string, ChartLabelConfiguration>
                        {
                            {
                                $"{indicatorId}_{filter1Id}_{filter2Id}_____",
                                new ChartLabelConfiguration
                                {
                                    Colour = "Test colour",
                                    Label = "Test label",
                                    Symbol = ChartLineSymbol.diamond,
                                    LineStyle = ChartLineStyle.dotted
                                }
                            }
                        },
                        Axes = new Dictionary<string, ChartAxisConfiguration>
                        {
                            {
                                "major",
                                new ChartAxisConfiguration
                                {
                                    DataSets = new List<ChartDataSet>
                                    {
                                        new ChartDataSet
                                        {
                                            Filters = new List<Guid> { filter1Id, filter2Id },
                                            Indicator = indicatorId
                                        }
                                    }
                                }
                            }
                        }
                    }
                },
            };

            var contextId = Guid.NewGuid().ToString();

            using (var context = ContentDbUtils.InMemoryContentDbContext(contextId))
            {
                context.Add(dataBlock);
                context.SaveChanges();
            }

            using (var context = ContentDbUtils.InMemoryContentDbContext(contextId))
            {
                var migration = new EES1598RemoveChartLabelsField(context, isTest: true);
                migration.Apply();
            }

            using (var context = ContentDbUtils.InMemoryContentDbContext(contextId))
            {
                var savedDataBlock = context.DataBlocks.Find(dataBlock.Id);

                Assert.Null(savedDataBlock.Charts[0].Labels);

                var dataSets = savedDataBlock.Charts[0].Axes?["major"].DataSets;

                Assert.NotNull(dataSets);
                Assert.Single(dataSets);

                Assert.Equal(indicatorId,  dataSets[0].Indicator);
                Assert.Equal(new List<Guid>{ filter1Id, filter2Id },  dataSets[0].Filters);

                Assert.Equal("Test colour", dataSets[0].Config.Colour);
                Assert.Equal("Test label", dataSets[0].Config.Label);
                Assert.Equal(ChartLineSymbol.diamond, dataSets[0].Config.Symbol);
                Assert.Equal(ChartLineStyle.dotted, dataSets[0].Config.LineStyle);
            }
        }

        [Fact]
        public void IgnoresDataSetsWithExistingConfig()
        {
            var indicatorId = Guid.NewGuid();
            var filter1Id = Guid.NewGuid();
            var filter2Id = Guid.NewGuid();

            var dataBlock = new DataBlock
            {
                Charts = new List<IChart>
                {
                    new LineChart
                    {
                        Labels = new Dictionary<string, ChartLabelConfiguration>
                        {
                            {
                                $"{indicatorId}_{filter1Id}_{filter2Id}_____",
                                new ChartLabelConfiguration
                                {
                                    Colour = "Updated colour",
                                    Label = "Updated label",
                                    Symbol = ChartLineSymbol.diamond,
                                    LineStyle = ChartLineStyle.dotted
                                }
                            }
                        },
                        Axes = new Dictionary<string, ChartAxisConfiguration>
                        {
                            {
                                "major",
                                new ChartAxisConfiguration
                                {
                                    DataSets = new List<ChartDataSet>
                                    {
                                        new ChartDataSet
                                        {
                                            Filters = new List<Guid> { filter1Id, filter2Id },
                                            Indicator = indicatorId,
                                            Config = new ChartDataSetConfiguration
                                            {
                                                Colour = "Original colour",
                                                Label = "Original label",
                                                Symbol = ChartLineSymbol.cross,
                                                LineStyle = ChartLineStyle.solid
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                },
            };

            var contextId = Guid.NewGuid().ToString();

            using (var context = ContentDbUtils.InMemoryContentDbContext(contextId))
            {
                context.Add(dataBlock);
                context.SaveChanges();
            }

            using (var context = ContentDbUtils.InMemoryContentDbContext(contextId))
            {
                var migration = new EES1598RemoveChartLabelsField(context, isTest: true);
                migration.Apply();
            }

            using (var context = ContentDbUtils.InMemoryContentDbContext(contextId))
            {
                var savedDataBlock = context.DataBlocks.Find(dataBlock.Id);

                Assert.Null(savedDataBlock.Charts[0].Labels);

                var dataSets = savedDataBlock.Charts[0].Axes?["major"].DataSets;

                Assert.NotNull(dataSets);
                Assert.Single(dataSets);

                Assert.Equal(indicatorId,  dataSets[0].Indicator);
                Assert.Equal(new List<Guid>{ filter1Id, filter2Id },  dataSets[0].Filters);

                Assert.Equal("Original colour", dataSets[0].Config.Colour);
                Assert.Equal("Original label", dataSets[0].Config.Label);
                Assert.Equal(ChartLineSymbol.cross, dataSets[0].Config.Symbol);
                Assert.Equal(ChartLineStyle.solid, dataSets[0].Config.LineStyle);
            }
        }

        [Fact]
        public void AddsAndUpdatesDataSets()
        {
            var indicator1Id = Guid.NewGuid();
            var indicator2Id = Guid.NewGuid();
            var filter1Id = Guid.NewGuid();
            var filter2Id = Guid.NewGuid();

            var dataBlock = new DataBlock
            {
                Charts = new List<IChart>
                {
                    new LineChart
                    {
                        Labels = new Dictionary<string, ChartLabelConfiguration>
                        {
                            {
                                $"{indicator1Id}_{filter1Id}_____",
                                new ChartLabelConfiguration
                                {
                                    Colour = "Test colour 1",
                                    Label = "Test label 1",
                                    Symbol = ChartLineSymbol.diamond,
                                    LineStyle = ChartLineStyle.dotted
                                }
                            },
                            {
                                $"{indicator2Id}_{filter2Id}_____",
                                new ChartLabelConfiguration
                                {
                                    Colour = "Test colour 2",
                                    Label = "Test label 2",
                                    Symbol = ChartLineSymbol.square,
                                    LineStyle = ChartLineStyle.solid
                                }
                            }
                        },
                        Axes = new Dictionary<string, ChartAxisConfiguration>
                        {
                            {
                                "major",
                                new ChartAxisConfiguration
                                {
                                    DataSets = new List<ChartDataSet>
                                    {
                                        new ChartDataSet
                                        {
                                            Filters = new List<Guid> { filter1Id },
                                            Indicator = indicator1Id
                                        }
                                    }
                                }
                            }
                        }
                    }
                },
            };

            var contextId = Guid.NewGuid().ToString();

            using (var context = ContentDbUtils.InMemoryContentDbContext(contextId))
            {
                context.Add(dataBlock);
                context.SaveChanges();
            }

            using (var context = ContentDbUtils.InMemoryContentDbContext(contextId))
            {
                var migration = new EES1598RemoveChartLabelsField(context, isTest: true);
                migration.Apply();
            }

            using (var context = ContentDbUtils.InMemoryContentDbContext(contextId))
            {
                var savedDataBlock = context.DataBlocks.Find(dataBlock.Id);

                Assert.Null(savedDataBlock.Charts[0].Labels);

                var dataSets = savedDataBlock.Charts[0].Axes?["major"].DataSets;

                Assert.NotNull(dataSets);
                Assert.Equal(2, dataSets.Count);

                Assert.Equal(indicator1Id,  dataSets[0].Indicator);
                Assert.Equal(new List<Guid>{ filter1Id },  dataSets[0].Filters);

                Assert.Equal("Test colour 1", dataSets[0].Config.Colour);
                Assert.Equal("Test label 1", dataSets[0].Config.Label);
                Assert.Equal(ChartLineSymbol.diamond, dataSets[0].Config.Symbol);
                Assert.Equal(ChartLineStyle.dotted, dataSets[0].Config.LineStyle);

                Assert.Equal(indicator2Id,  dataSets[1].Indicator);
                Assert.Equal(new List<Guid> { filter2Id },  dataSets[1].Filters);

                Assert.Equal("Test colour 2", dataSets[1].Config.Colour);
                Assert.Equal("Test label 2", dataSets[1].Config.Label);
                Assert.Equal(ChartLineSymbol.square, dataSets[1].Config.Symbol);
                Assert.Equal(ChartLineStyle.solid, dataSets[1].Config.LineStyle);
            }
        }

        [Fact]
        public void IgnoresJunkLabels()
        {
            var indicatorId = Guid.NewGuid();
            var filter1Id = Guid.NewGuid();
            var filter2Id = Guid.NewGuid();

            var junkLabelConfig = new ChartLabelConfiguration
            {
                Colour = "junk-colour",
                Label = "Junk label",
                Symbol = ChartLineSymbol.cross,
                LineStyle = ChartLineStyle.dashed
            };

            var dataBlock = new DataBlock
            {
                Charts = new List<IChart>
                {
                    new LineChart
                    {
                        Labels = new Dictionary<string, ChartLabelConfiguration>
                        {
                            {
                                $"{indicatorId}_{filter1Id}_{filter2Id}_____",
                                new ChartLabelConfiguration
                                {
                                    Colour = "Test colour 1",
                                    Label = "Test label 1",
                                    Symbol = ChartLineSymbol.diamond,
                                    LineStyle = ChartLineStyle.dotted
                                }
                            },
                            {
                                $"{indicatorId}_____",
                                new ChartLabelConfiguration
                                {
                                    Colour = "Test colour 2",
                                    Label = "Test label 2",
                                    Symbol = ChartLineSymbol.square,
                                    LineStyle = ChartLineStyle.solid
                                }
                            },
                            {
                                $"2015_AY",
                                junkLabelConfig
                            },
                            {
                                "_____",
                                junkLabelConfig
                            },
                            {
                                "something-that-does-not-make-sense-but-is-long-enough-to-be-a-guid",
                                junkLabelConfig
                            }
                        },
                        Axes = new Dictionary<string, ChartAxisConfiguration>
                        {
                            {
                                "major",
                                new ChartAxisConfiguration
                                {
                                    DataSets = new List<ChartDataSet>()
                                }
                            }
                        }
                    }
                },
            };

            var contextId = Guid.NewGuid().ToString();

            using (var context = ContentDbUtils.InMemoryContentDbContext(contextId))
            {
                context.Add(dataBlock);
                context.SaveChanges();
            }

            using (var context = ContentDbUtils.InMemoryContentDbContext(contextId))
            {
                var migration = new EES1598RemoveChartLabelsField(context, isTest: true);
                migration.Apply();
            }

            using (var context = ContentDbUtils.InMemoryContentDbContext(contextId))
            {
                var savedDataBlock = context.DataBlocks.Find(dataBlock.Id);

                Assert.Null(savedDataBlock.Charts[0].Labels);

                var dataSets = savedDataBlock.Charts[0].Axes?["major"].DataSets;

                Assert.NotNull(dataSets);
                Assert.Equal(2, dataSets.Count);

                Assert.Equal(indicatorId,  dataSets[0].Indicator);
                Assert.Equal(new List<Guid>{ filter1Id, filter2Id },  dataSets[0].Filters);

                Assert.Equal("Test colour 1", dataSets[0].Config.Colour);
                Assert.Equal("Test label 1", dataSets[0].Config.Label);
                Assert.Equal(ChartLineSymbol.diamond, dataSets[0].Config.Symbol);
                Assert.Equal(ChartLineStyle.dotted, dataSets[0].Config.LineStyle);

                Assert.Equal(indicatorId,  dataSets[1].Indicator);
                Assert.Equal(new List<Guid>(),  dataSets[1].Filters);

                Assert.Equal("Test colour 2", dataSets[1].Config.Colour);
                Assert.Equal("Test label 2", dataSets[1].Config.Label);
                Assert.Equal(ChartLineSymbol.square, dataSets[1].Config.Symbol);
                Assert.Equal(ChartLineStyle.solid, dataSets[1].Config.LineStyle);
            }
        }

        [Fact]
        public void MultipleDataBlocks()
        {
            var indicatorId = Guid.NewGuid();
            var filter1Id = Guid.NewGuid();
            var filter2Id = Guid.NewGuid();

            var dataBlock1 = new DataBlock
            {
                Charts = new List<IChart>
                {
                    new LineChart
                    {
                        Labels = new Dictionary<string, ChartLabelConfiguration>
                        {
                            {
                                $"{indicatorId}_{filter1Id}_____",
                                new ChartLabelConfiguration
                                {
                                    Colour = "Test colour 1",
                                    Label = "Test label 1",
                                    Symbol = ChartLineSymbol.diamond,
                                    LineStyle = ChartLineStyle.dotted
                                }
                            }
                        },
                        Axes = new Dictionary<string, ChartAxisConfiguration>
                        {
                            {
                                "major",
                                new ChartAxisConfiguration
                                {
                                    DataSets = new List<ChartDataSet>()
                                }
                            }
                        }
                    }
                },
            };

            var dataBlock2 = new DataBlock
            {
                Charts = new List<IChart>
                {
                    new LineChart
                    {
                        Labels = new Dictionary<string, ChartLabelConfiguration>
                        {
                            {
                                $"{indicatorId}_{filter2Id}_____",
                                new ChartLabelConfiguration
                                {
                                    Colour = "Test colour 2",
                                    Label = "Test label 2",
                                    Symbol = ChartLineSymbol.square,
                                    LineStyle = ChartLineStyle.solid
                                }
                            }
                        },
                        Axes = new Dictionary<string, ChartAxisConfiguration>
                        {
                            {
                                "major",
                                new ChartAxisConfiguration
                                {
                                    DataSets = new List<ChartDataSet>()
                                }
                            }
                        }
                    }
                },
            };

            var contextId = Guid.NewGuid().ToString();

            using (var context = ContentDbUtils.InMemoryContentDbContext(contextId))
            {
                context.AddRange(dataBlock1, dataBlock2);
                context.SaveChanges();
            }

            using (var context = ContentDbUtils.InMemoryContentDbContext(contextId))
            {
                var migration = new EES1598RemoveChartLabelsField(context, isTest: true);
                migration.Apply();
            }

            using (var context = ContentDbUtils.InMemoryContentDbContext(contextId))
            {
                var dataBlocks = context.DataBlocks.ToList();

                Assert.Equal(2, dataBlocks.Count);

                Assert.Null(dataBlocks[0].Charts[0].Labels);

                var dataBlock1DataSets = dataBlocks[0].Charts[0].Axes?["major"].DataSets;

                Assert.NotNull(dataBlock1DataSets);
                Assert.Single(dataBlock1DataSets);

                Assert.Equal(indicatorId, dataBlock1DataSets[0].Indicator);
                Assert.Equal(new List<Guid> {filter1Id}, dataBlock1DataSets[0].Filters);

                Assert.Equal("Test colour 1", dataBlock1DataSets[0].Config.Colour);
                Assert.Equal("Test label 1", dataBlock1DataSets[0].Config.Label);
                Assert.Equal(ChartLineSymbol.diamond, dataBlock1DataSets[0].Config.Symbol);
                Assert.Equal(ChartLineStyle.dotted, dataBlock1DataSets[0].Config.LineStyle);

                Assert.Null(dataBlocks[1].Charts[0].Labels);

                var dataBlock2DataSets = dataBlocks[1].Charts[0].Axes?["major"].DataSets;

                Assert.NotNull(dataBlock2DataSets);
                Assert.Single(dataBlock2DataSets);

                Assert.Equal(indicatorId, dataBlock2DataSets[0].Indicator);
                Assert.Equal(new List<Guid> {filter2Id}, dataBlock2DataSets[0].Filters);

                Assert.Equal("Test colour 2", dataBlock2DataSets[0].Config.Colour);
                Assert.Equal("Test label 2", dataBlock2DataSets[0].Config.Label);
                Assert.Equal(ChartLineSymbol.square, dataBlock2DataSets[0].Config.Symbol);
                Assert.Equal(ChartLineStyle.solid, dataBlock2DataSets[0].Config.LineStyle);
            }
        }
    }
}