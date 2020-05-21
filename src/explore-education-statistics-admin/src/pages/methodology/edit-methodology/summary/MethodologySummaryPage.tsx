import ButtonLink from '@admin/components/ButtonLink';
import { useMethodologyState } from '@admin/pages/methodology/edit-methodology/content/context/MethodologyContext';
import methodologyService from '@admin/services/methodology/methodologyService';
import FormattedDate from '@common/components/FormattedDate';
import LoadingSpinner from '@common/components/LoadingSpinner';
import SummaryList from '@common/components/SummaryList';
import SummaryListItem from '@common/components/SummaryListItem';
import Tag from '@common/components/Tag';
import WarningMessage from '@common/components/WarningMessage';
import useAsyncHandledRetry from '@common/hooks/useAsyncHandledRetry';
import React from 'react';

const MethodologySummaryPage = () => {
  const { methodology } = useMethodologyState();

  const { value: currentMethodology, isLoading } = useAsyncHandledRetry(() =>
    methodologyService.getMethodology(methodology.id),
  );

  return (
    <>
      <h2>Methodology summary</h2>

      <LoadingSpinner loading={isLoading}>
        {currentMethodology ? (
          <>
            <SummaryList>
              <SummaryListItem term="Title">
                {currentMethodology.title}
              </SummaryListItem>
              <SummaryListItem term="Status">
                <Tag>{currentMethodology.status}</Tag>
              </SummaryListItem>
              <SummaryListItem term="Scheduled release">
                <FormattedDate>
                  {currentMethodology.publishScheduled}
                </FormattedDate>
              </SummaryListItem>
              <SummaryListItem term="Published on">
                {currentMethodology.published ? (
                  <FormattedDate>{currentMethodology.published}</FormattedDate>
                ) : (
                  'Not yet published'
                )}
              </SummaryListItem>
            </SummaryList>

            {currentMethodology.status !== 'Approved' && (
              <ButtonLink to={`/methodologies/${methodology.id}/summary/edit`}>
                Edit summary
              </ButtonLink>
            )}
          </>
        ) : (
          <WarningMessage>
            There was a problem loading the methodology summary.
          </WarningMessage>
        )}
      </LoadingSpinner>
    </>
  );
};

export default MethodologySummaryPage;
