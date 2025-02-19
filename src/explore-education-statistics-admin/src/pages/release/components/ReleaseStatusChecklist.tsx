import Link from '@admin/components/Link';
import releaseDataPageTabIds from '@admin/pages/release/data/utils/releaseDataPageTabIds';
import { releasePreReleaseAccessPageTabs } from '@admin/pages/release/pre-release/ReleasePreReleaseAccessPage';
import {
  MethodologyRouteParams,
  methodologyStatusRoute,
} from '@admin/routes/methodologyRoutes';
import {
  releaseContentRoute,
  releaseDataBlocksRoute,
  releaseDataRoute,
  releaseFootnotesRoute,
  releasePreReleaseAccessRoute,
  ReleaseRouteParams,
} from '@admin/routes/releaseRoutes';
import { PublicationRouteParams } from '@admin/routes/routes';
import {
  ReleaseVersion,
  ReleaseVersionChecklist,
  ReleaseVersionChecklistError,
  ReleaseVersionChecklistWarning,
} from '@admin/services/releaseVersionService';
import InsetText from '@common/components/InsetText';
import React, { useMemo } from 'react';
import { generatePath } from 'react-router';
import { publicationMethodologiesRoute } from '@admin/routes/publicationRoutes';
import { useAuthContext } from '@admin/contexts/AuthContext';
import { formId } from './ReleaseStatusForm';

interface ChecklistMessage {
  message: string;
  link?: string;
}

interface Props {
  checklist: ReleaseVersionChecklist;
  releaseVersion: ReleaseVersion;
}

const ReleaseStatusChecklist = ({ checklist, releaseVersion }: Props) => {
  const { user } = useAuthContext();

  const releaseRouteParams = useMemo<ReleaseRouteParams>(
    () => ({
      releaseVersionId: releaseVersion.id,
      publicationId: releaseVersion.publicationId,
    }),
    [releaseVersion.id, releaseVersion.publicationId],
  );

  const errors = useMemo<ChecklistMessage[]>(() => {
    const dataUploadsTabRoute = `${generatePath<ReleaseRouteParams>(
      releaseDataRoute.path,
      releaseRouteParams,
    )}#${releaseDataPageTabIds.dataUploads}`;

    const apiDataSetsTabRoute = user?.permissions.isBauUser
      ? `${generatePath<ReleaseRouteParams>(
          releaseDataRoute.path,
          releaseRouteParams,
        )}#${releaseDataPageTabIds.apiDataSets}`
      : undefined;

    return checklist.errors.map(error => {
      switch (error.code) {
        case 'DataFileImportsMustBeCompleted':
          return {
            message: 'All data imports must be completed',
            link: dataUploadsTabRoute,
          };
        case 'DataFileReplacementsMustBeCompleted':
          return {
            message: 'All data file replacements must be completed',
            link: dataUploadsTabRoute,
          };
        case 'PublicDataGuidanceRequired':
          return {
            message:
              'All summary information must be completed on the data guidance page',
            link: `${generatePath<ReleaseRouteParams>(
              releaseDataRoute.path,
              releaseRouteParams,
            )}#${releaseDataPageTabIds.dataGuidance}`,
          };
        case 'ReleaseNoteRequired':
          return {
            message:
              'A public release note for this amendment is required, add this near the top of the content page',
            link: generatePath<ReleaseRouteParams>(
              releaseContentRoute.path,
              releaseRouteParams,
            ),
          };
        case 'EmptyContentSectionExists':
          return {
            message: 'Release content should not contain any empty sections',
            link: generatePath<ReleaseRouteParams>(
              releaseContentRoute.path,
              releaseRouteParams,
            ),
          };
        case 'GenericSectionsContainEmptyHtmlBlock':
          return {
            message: 'Release content should not contain empty text blocks',
            link: generatePath<ReleaseRouteParams>(
              releaseContentRoute.path,
              releaseRouteParams,
            ),
          };
        case 'RelatedDashboardsSectionContainsEmptyHtmlBlock':
          return {
            message:
              'Release content should not contain an empty related dashboards section',
            link: generatePath<ReleaseRouteParams>(
              releaseContentRoute.path,
              releaseRouteParams,
            ),
          };
        case 'ReleaseMustContainKeyStatOrNonEmptyHeadlineBlock':
          return {
            message:
              'Release must contain a key statistic or a non-empty headline text block',
            link: generatePath<ReleaseRouteParams>(
              releaseContentRoute.path,
              releaseRouteParams,
            ),
          };
        case 'SummarySectionContainsEmptyHtmlBlock':
          return {
            message:
              'Release content should not contain an empty summary section',
            link: generatePath<ReleaseRouteParams>(
              releaseContentRoute.path,
              releaseRouteParams,
            ),
          };
        case 'PublicApiDataSetImportsMustBeCompleted':
          return {
            message: 'All public API data set processing must be completed',
            link: apiDataSetsTabRoute,
          };
        case 'PublicApiDataSetCancellationsMustBeResolved':
          return {
            message:
              'All cancelled public API data sets must be removed or completed',
            link: apiDataSetsTabRoute,
          };
        case 'PublicApiDataSetFailuresMustBeResolved':
          return {
            message:
              'All failed public API data sets must be retried or removed',
            link: apiDataSetsTabRoute,
          };
        case 'PublicApiDataSetMappingsMustBeCompleted':
          return {
            message: 'All public API data set mappings must be completed',
            link: apiDataSetsTabRoute,
          };
        default:
          // Show error code, even if there is no mapping,
          // as this is better than having invisible errors.
          return {
            message: (error as ReleaseVersionChecklistError).code,
          };
      }
    });
  }, [checklist.errors, releaseRouteParams, user?.permissions.isBauUser]);

  const warnings = useMemo<ChecklistMessage[]>(() => {
    return checklist.warnings.map(warning => {
      switch (warning.code) {
        case 'MethodologyNotApproved':
          return {
            message: 'A methodology for this publication is not yet approved',
            link: generatePath<MethodologyRouteParams>(
              methodologyStatusRoute.path,
              {
                methodologyId: warning.methodologyId,
              },
            ),
          };
        case 'NoDataFiles':
          return {
            message: 'No data files uploaded',
            link: `${generatePath<ReleaseRouteParams>(
              releaseDataRoute.path,
              releaseRouteParams,
            )}#${releaseDataPageTabIds.dataUploads}`,
          };
        case 'NoFootnotesOnSubjects':
          return {
            message: `${
              warning.totalSubjects === 1
                ? '1 data file does not have any footnotes'
                : `${warning.totalSubjects} data files don't have any footnotes`
            }`,
            link: generatePath<ReleaseRouteParams>(
              releaseFootnotesRoute.path,
              releaseRouteParams,
            ),
          };
        case 'NoMethodology':
          return {
            message:
              'An in-EES methodology page has not been linked to this publication',
            link: generatePath<PublicationRouteParams>(
              publicationMethodologiesRoute.path,
              { publicationId: releaseVersion.publicationId },
            ),
          };
        case 'NoNextReleaseDate':
          return {
            message: 'No next expected release date has been added',
            link: `#${formId}-nextReleaseDate-month`,
          };
        case 'NoFeaturedTables':
          return {
            message: 'No data blocks have been saved as featured tables',
            link: generatePath<ReleaseRouteParams>(
              releaseDataBlocksRoute.path,
              releaseRouteParams,
            ),
          };
        case 'NoPublicPreReleaseAccessList':
          return {
            message: 'A public pre-release access list has not been created',
            link: `${generatePath<ReleaseRouteParams>(
              releasePreReleaseAccessRoute.path,
              releaseRouteParams,
            )}#${releasePreReleaseAccessPageTabs.publicAccessList}`,
          };
        default:
          // Show warning code, even if there is no mapping,
          // as this is better than having invisible warnings.
          return {
            message: (warning as ReleaseVersionChecklistWarning).code,
          };
      }
    });
  }, [checklist.warnings, releaseVersion.publicationId, releaseRouteParams]);

  return (
    <div className="govuk-!-width-two-thirds">
      <h3>Publishing checklist</h3>

      {errors.length === 0 && checklist.warnings.length === 0 && (
        <InsetText variant="success" testId="releaseChecklist-success">
          <h4 className="govuk-heading-m">All checks passed</h4>

          <p>No issues to resolve. This release can be published.</p>
        </InsetText>
      )}

      {errors.length > 0 && (
        <InsetText variant="error" testId="releaseChecklist-errors">
          <h4 className="govuk-heading-m">Errors</h4>

          <p>
            <strong>
              {`${errors.length} ${errors.length === 1 ? 'issue' : 'issues'}`}
            </strong>{' '}
            that must be resolved before this release can be published.
          </p>

          <ul>
            {errors.map(error => (
              <li key={`${error.message}${error.link}`}>
                {error.link ? (
                  <Link to={error.link} unvisited>
                    {error.message}
                  </Link>
                ) : (
                  error.message
                )}
              </li>
            ))}
          </ul>
        </InsetText>
      )}

      {warnings.length > 0 && (
        <InsetText variant="warning" testId="releaseChecklist-warnings">
          <h4 className="govuk-heading-m">Warnings</h4>

          <p>
            <strong>
              {`${warnings.length} ${
                warnings.length === 1 ? 'thing' : 'things'
              }`}
            </strong>{' '}
            you may have forgotten, but do not need to resolve to publish this
            release.
          </p>

          <ul>
            {warnings.map(warning => (
              <li key={`${warning.message}${warning.link}`}>
                {warning.link ? (
                  <Link to={warning.link} unvisited>
                    {warning.message}
                  </Link>
                ) : (
                  warning.message
                )}
              </li>
            ))}
          </ul>
        </InsetText>
      )}
    </div>
  );
};

export default ReleaseStatusChecklist;
