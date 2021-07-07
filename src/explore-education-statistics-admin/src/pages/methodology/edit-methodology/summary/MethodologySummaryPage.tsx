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
import {
  ReleaseRouteParams,
  releaseSummaryRoute,
} from '@admin/routes/releaseRoutes';
import React from 'react';
import { generatePath, RouteComponentProps } from 'react-router';
import { Link } from 'react-router-dom';

const myFakeMethodology = {
  id: 'm1',
  live: false,
  title: 'My fake methodology',
  slug: 'my-fake-methodology',
  status: 'Draft',
  publication: {
    id: 'p1',
    title: 'owner publication title',
    releaseId: 'r1',
  },
  published: '',
  otherPublications: [
    {
      id: 'op1',
      title: 'other publication title 1',
      releaseId: 'r1',
    },
    {
      id: 'op2',
      title: 'other publication title 2',
      releaseId: 'r1',
    },
    {
      id: 'op3',
      title: 'other publication title 3',
      releaseId: 'r1',
    },
  ],
};

const MethodologySummaryPage = ({
  match,
}: RouteComponentProps<MethodologyRouteParams>) => {
  const { methodologyId } = match.params;

  const { value: realMethodology, isLoading } = useAsyncHandledRetry(() =>
    methodologyService.getMethodology(methodologyId),
  );

  // EES-2161 - flip showPublications to see the publications. Remove when BE done and change to alsways use real methodology.
  const showPublications = false;
  const currentMethodology = showPublications
    ? myFakeMethodology
    : realMethodology;

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
              {currentMethodology.publication && (
                <SummaryListItem term="Owning publication">
                  <Link
                    to={generatePath<ReleaseRouteParams>(
                      releaseSummaryRoute.path,
                      {
                        publicationId: currentMethodology.publication.id,
                        releaseId: currentMethodology.publication.releaseId,
                      },
                    )}
                  >
                    {currentMethodology.publication.title}
                  </Link>
                </SummaryListItem>
              )}
              {currentMethodology.otherPublications &&
                currentMethodology.otherPublications.length > 0 && (
                  <SummaryListItem term="Other publications">
                    <ul className="govuk-!-margin-top-0">
                      {currentMethodology.otherPublications?.map(
                        publication => (
                          <li key={publication.id}>
                            <Link
                              to={generatePath<ReleaseRouteParams>(
                                releaseSummaryRoute.path,
                                {
                                  publicationId: publication.id,
                                  releaseId: publication.releaseId,
                                },
                              )}
                            >
                              {publication.title}
                            </Link>
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
