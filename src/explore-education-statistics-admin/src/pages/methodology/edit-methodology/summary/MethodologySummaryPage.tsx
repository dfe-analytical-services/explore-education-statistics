import ButtonLink from '@admin/components/ButtonLink';
import { MethodologyRouteParams } from '@admin/routes/methodologyRoutes';
import methodologyService from '@admin/services/methodologyService';
import FormattedDate from '@common/components/FormattedDate';
import LoadingSpinner from '@common/components/LoadingSpinner';
import SummaryList from '@common/components/SummaryList';
import SummaryListItem from '@common/components/SummaryListItem';
import Tag from '@common/components/Tag';
import WarningMessage from '@common/components/WarningMessage';
import useAsyncHandledRetry from '@common/hooks/useAsyncHandledRetry';
import { parseISO } from 'date-fns';
import React from 'react';
import { RouteComponentProps } from 'react-router';

const MethodologySummaryPage = ({
  match,
}: RouteComponentProps<MethodologyRouteParams>) => {
  const { methodologyId } = match.params;

  const { value: currentMethodology, isLoading } = useAsyncHandledRetry(() =>
    methodologyService.getMethodology(methodologyId),
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
                  {parseISO(currentMethodology.publishScheduled)}
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
              <ButtonLink to={`/methodologies/${methodologyId}/summary/edit`}>
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
