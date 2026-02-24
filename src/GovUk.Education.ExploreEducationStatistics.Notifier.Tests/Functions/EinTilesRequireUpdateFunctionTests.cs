using GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;
using GovUk.Education.ExploreEducationStatistics.Notifier.Functions;
using GovUk.Education.ExploreEducationStatistics.Notifier.Model;
using GovUk.Education.ExploreEducationStatistics.Notifier.Options;
using GovUk.Education.ExploreEducationStatistics.Notifier.Services.Interfaces;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace GovUk.Education.ExploreEducationStatistics.Notifier.Tests.Functions;

public class EinTilesRequireUpdateFunctionTests
{
    private static readonly AppOptions AppOptions = new()
    {
        AdminAppUrl = "https://admin.url",
        PublicAppUrl = "https://public.url",
        BauEmail = "bau@example.com",
    };

    private static readonly GovUkNotifyOptions.EmailTemplateOptions EmailTemplateOptions = new()
    {
        EinTilesRequireUpdateId = "ein-tiles-require-update-id",
    };

    [Fact]
    public void Success()
    {
        var page1Title = "Page 1 title";
        var page1Id = Guid.NewGuid();

        var tile1Title = "Tile 1 title";
        var tile1ContentSectionTitle = "Tile 1 content section title";
        var tile1DataSetFileId = Guid.NewGuid();

        var page2Title = "Page 2 title";
        var page2Id = Guid.NewGuid();

        var tile2Title = "Tile 2 title";
        var tile2ContentSectionTitle = "Tile 2 content section title";
        var tile2DataSetFileId = Guid.NewGuid();

        var tile3Title = "Tile 3 title";
        var tile3ContentSectionTitle = "Tile 3 content section title";
        var tile3DataSetFileId = Guid.NewGuid();

        var expectedBulletList = $"""
            * {page1Title}, {AppOptions.AdminAppUrl}/education-in-numbers/{page1Id}/content
              * Tile titled '{tile1Title}' in section '{tile1ContentSectionTitle}', which uses this data set: {AppOptions.PublicAppUrl}/data-catalogue/data-set/{tile1DataSetFileId}
            * {page2Title}, {AppOptions.AdminAppUrl}/education-in-numbers/{page2Id}/content
              * Tile titled '{tile2Title}' in section '{tile2ContentSectionTitle}', which uses this data set: {AppOptions.PublicAppUrl}/data-catalogue/data-set/{tile2DataSetFileId}
              * Tile titled '{tile3Title}' in section '{tile3ContentSectionTitle}', which uses this data set: {AppOptions.PublicAppUrl}/data-catalogue/data-set/{tile3DataSetFileId}

            """;

        var emailService = new Mock<IEmailService>();
        emailService.Setup(mock =>
            mock.SendEmail(
                AppOptions.BauEmail,
                EmailTemplateOptions.EinTilesRequireUpdateId,
                It.Is<Dictionary<string, dynamic>>(templateValues =>
                    AssertEmailTemplateValues(templateValues, expectedBulletList)
                )
            )
        );

        var function = BuildFunction(emailService: emailService.Object);

        function.SendEinTilesNeedUpdateEmail(
            new EinTilesRequireUpdateMessage
            {
                Pages =
                [
                    new EinPageRequiresUpdate
                    {
                        Id = page1Id,
                        Title = page1Title,
                        Tiles =
                        [
                            new EinTileRequiresUpdate
                            {
                                Title = tile1Title,
                                ContentSectionTitle = tile1ContentSectionTitle,
                                DataSetFileId = tile1DataSetFileId,
                            },
                        ],
                    },
                    new EinPageRequiresUpdate
                    {
                        Id = page2Id,
                        Title = page2Title,
                        Tiles =
                        [
                            new EinTileRequiresUpdate
                            {
                                Title = tile2Title,
                                ContentSectionTitle = tile2ContentSectionTitle,
                                DataSetFileId = tile2DataSetFileId,
                            },
                            new EinTileRequiresUpdate
                            {
                                Title = tile3Title,
                                ContentSectionTitle = tile3ContentSectionTitle,
                                DataSetFileId = tile3DataSetFileId,
                            },
                        ],
                    },
                ],
            },
            CancellationToken.None
        );
    }

    private static bool AssertEmailTemplateValues(Dictionary<string, dynamic> templateValues, string updateBulletList)
    {
        Assert.Equal(updateBulletList, templateValues["update_bullet_list"]);
        return true;
    }

    private static EinTilesRequireUpdateFunction BuildFunction(
        IEmailService? emailService = null,
        ILogger<EinTilesRequireUpdateFunction>? logger = null
    )
    {
        return new EinTilesRequireUpdateFunction(
            AppOptions.ToOptionsWrapper(),
            new GovUkNotifyOptions { ApiKey = "", EmailTemplates = EmailTemplateOptions }.ToOptionsWrapper(),
            emailService ?? Mock.Of<IEmailService>(MockBehavior.Strict),
            logger ?? Mock.Of<ILogger<EinTilesRequireUpdateFunction>>(MockBehavior.Strict)
        );
    }
}
