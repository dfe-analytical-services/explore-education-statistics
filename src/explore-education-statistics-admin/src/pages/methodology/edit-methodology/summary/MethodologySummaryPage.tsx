import ButtonLink from '@admin/components/ButtonLink';
import {
  MethodologyRouteParams,
  methodologySummaryEditRoute,
} from '@admin/routes/methodologyRoutes';
import methodologyService from '@admin/services/methodologyService';
import FormattedDate from '@common/components/FormattedDate';
import LoadingSpinner from '@common/components/LoadingSpinner';
import SummaryList from '@common/components/SummaryList';
import SummaryListItem from '@common/components/SummaryListItem';
import Tag from '@common/components/Tag';
import WarningMessage from '@common/components/WarningMessage';
import useAsyncHandledRetry from '@common/hooks/useAsyncHandledRetry';
import React from 'react';
import { generatePath, RouteComponentProps } from 'react-router';

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
                {currentMethodology.amendment && (
                  <>
                    {' '}
                    <Tag>Amendment</Tag>
                  </>
                )}
              </SummaryListItem>
              <SummaryListItem term="Published on">
                {currentMethodology.published ? (
                  <FormattedDate>{currentMethodology.published}</FormattedDate>
                ) : (
                  'Not yet published'
                )}
              </SummaryListItem>
              <SummaryListItem term="Owning publication">
                {currentMethodology.owningPublication.title}
              </SummaryListItem>
              {currentMethodology.otherPublications &&
                currentMethodology.otherPublications.length > 0 && (
                  <SummaryListItem term="Other publications">
                    <ul className="govuk-!-margin-top-0">
                      {currentMethodology.otherPublications?.map(
                        publication => (
                          <li
                            key={publication.id}
                            data-testid="other-publication-item"
                          >
                            {publication.title}
                          </li>
                        ),
                      )}
                    </ul>
                  </SummaryListItem>
                )}
            </SummaryList>

            {currentMethodology.status !== 'Approved' && (
              <ButtonLink
                to={generatePath<MethodologyRouteParams>(
                  methodologySummaryEditRoute.path,
                  {
                    methodologyId,
                  },
                )}
              >
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
