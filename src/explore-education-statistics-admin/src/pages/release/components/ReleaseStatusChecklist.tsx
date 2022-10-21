import Link from '@admin/components/Link';
import { releaseDataPageTabIds } from '@admin/pages/release/data/ReleaseDataPage';
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
  Release,
  ReleaseChecklist,
  ReleaseChecklistError,
  ReleaseChecklistWarning,
} from '@admin/services/releaseService';
import InsetText from '@common/components/InsetText';
import React, { useMemo } from 'react';
import { generatePath } from 'react-router';
import { publicationMethodologiesRoute } from '@admin/routes/publicationRoutes';
import { formId } from './ReleaseStatusForm';

interface ChecklistMessage {
  message: string;
  link?: string;
}

interface Props {
  checklist: ReleaseChecklist;
  release: Release;
}

const ReleaseStatusChecklist = ({ checklist, release }: Props) => {
  const releaseRouteParams = useMemo<ReleaseRouteParams>(
    () => ({
      releaseId: release.id,
      publicationId: release.publicationId,
    }),
    [release.id, release.publicationId],
  );

  const errors = useMemo<ChecklistMessage[]>(() => {
    return checklist.errors.map(error => {
      switch (error.code) {
        case 'DataFileImportsMustBeCompleted':
          return {
            message: 'All data imports must be completed',
            link: `${generatePath<ReleaseRouteParams>(
              releaseDataRoute.path,
              releaseRouteParams,
            )}#${releaseDataPageTabIds.dataUploads}`,
          };
        case 'DataFileReplacementsMustBeCompleted':
          return {
            message: 'All data file replacements must be completed',
            link: `${generatePath<ReleaseRouteParams>(
              releaseDataRoute.path,
              releaseRouteParams,
            )}#${releaseDataPageTabIds.dataUploads}`,
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
        default:
          // Show error code, even if there is no mapping,
          // as this is better than having invisible errors.
          return {
            message: (error as ReleaseChecklistError).code,
          };
      }
    });
  }, [checklist.errors, releaseRouteParams]);

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
              { publicationId: release.publicationId },
            ),
          };
        case 'NoNextReleaseDate':
          return {
            message: 'No next expected release date has been added',
            link: `#${formId}-nextReleaseDate-month`,
          };
        case 'NoTableHighlights':
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
            message: (warning as ReleaseChecklistWarning).code,
          };
      }
    });
  }, [checklist.warnings, release.publicationId, releaseRouteParams]);

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
