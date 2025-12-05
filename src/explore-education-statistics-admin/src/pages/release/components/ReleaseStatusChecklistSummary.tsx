import releaseQueries from '@admin/queries/releaseQueries';
import {
  ReleaseRouteParams,
  releaseChecklistRoute,
} from '@admin/routes/releaseRoutes';
import InsetText from '@common/components/InsetText';
import LoadingSpinner from '@common/components/LoadingSpinner';
import VisuallyHidden from '@common/components/VisuallyHidden';
import React from 'react';
import { useQuery } from '@tanstack/react-query';
import { generatePath } from 'react-router';
import { Link } from 'react-router-dom';

interface Props {
  publicationId: string;
  releaseTitle?: string;
  releaseVersionId: string;
  simple?: boolean;
}

export default function ReleaseStatusChecklistSummary({
  publicationId,
  releaseTitle,
  releaseVersionId,
  simple = false,
}: Props) {
  const { data: checklist, isFetching } = useQuery(
    releaseQueries.getChecklist(releaseVersionId),
  );

  const { errors = [], warnings = [] } = checklist ?? {};

  const totalErrors = errors.length;
  const totalWarnings = warnings.length;
  const totalIssues = totalErrors + totalWarnings;

  const getSummaryText = () => {
    if (totalErrors) {
      const warningsText =
        totalWarnings === 1
          ? ' (and 1 warning)'
          : ` (and ${totalWarnings} warnings)`;
      return totalErrors === 1
        ? `There is 1 error${
            totalWarnings ? warningsText : ''
          }, please resolve this before assigning for higher review.`
        : `There are ${totalErrors} errors${
            totalWarnings ? warningsText : ''
          }, please resolve these before assigning for higher review.`;
    }

    return totalWarnings === 1
      ? 'There is 1 warning, please check this before assigning for higher review.'
      : `There are ${totalWarnings} warnings, please check these before assigning for higher review.`;
  };

  const getColour = () => {
    if (totalErrors) {
      return 'error';
    }
    return totalWarnings ? 'warning' : 'success';
  };

  if (simple) {
    return (
      <LoadingSpinner inline loading={isFetching} size="sm">
        {totalIssues > 0 ? (
          <Link
            to={generatePath<ReleaseRouteParams>(releaseChecklistRoute.path, {
              publicationId,
              releaseVersionId,
            })}
          >
            {`View issues (${totalIssues})`}
            <VisuallyHidden>
              {` for ${releaseTitle ?? 'this release'}`}
            </VisuallyHidden>
          </Link>
        ) : (
          'No issues'
        )}
      </LoadingSpinner>
    );
  }

  return (
    <>
      <h3>Publishing checklist</h3>
      <LoadingSpinner loading={isFetching}>
        <InsetText variant={getColour()}>
          {totalIssues > 0 ? (
            <Link
              to={generatePath<ReleaseRouteParams>(releaseChecklistRoute.path, {
                publicationId,
                releaseVersionId,
              })}
            >
              {getSummaryText()}
            </Link>
          ) : (
            <p>No issues to resolve. This release can be published.</p>
          )}
        </InsetText>
      </LoadingSpinner>
    </>
  );
}
