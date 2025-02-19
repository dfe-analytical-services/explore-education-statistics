import releaseVersionService from '@admin/services/releaseVersionService';
import Details from '@common/components/Details';
import LoadingSpinner from '@common/components/LoadingSpinner';
import Tag from '@common/components/Tag';
import useAsyncHandledRetry from '@common/hooks/useAsyncHandledRetry';
import React from 'react';
import TagGroup from '@common/components/TagGroup';

interface Props {
  releaseVersionId: string;
}

const DraftReleaseRowIssues = ({ releaseVersionId }: Props) => {
  const { value: checklist, isLoading: isLoadingChecklist } =
    useAsyncHandledRetry(() =>
      releaseVersionService.getReleaseVersionChecklist(releaseVersionId),
    );

  const totalIssues = checklist
    ? checklist.errors.length + checklist.warnings.length
    : 0;

  return (
    <td>
      <LoadingSpinner inline loading={isLoadingChecklist} size="sm">
        {totalIssues === 0 ? (
          'No issues'
        ) : (
          <Details
            className="govuk-!-margin-bottom-0"
            summary={`View issues (${totalIssues})`}
          >
            <TagGroup>
              <Tag colour="red">{`${checklist?.errors.length} errors`}</Tag>
              <Tag colour="yellow">{`${checklist?.warnings.length} warnings`}</Tag>
            </TagGroup>
          </Details>
        )}
      </LoadingSpinner>
    </td>
  );
};

export default DraftReleaseRowIssues;
