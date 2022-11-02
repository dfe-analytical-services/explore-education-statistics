import ButtonLink from '@admin/components/ButtonLink';
import {
  MethodologyRouteParams,
  methodologySummaryEditRoute,
} from '@admin/routes/methodologyRoutes';
import FormattedDate from '@common/components/FormattedDate';
import SummaryList from '@common/components/SummaryList';
import SummaryListItem from '@common/components/SummaryListItem';
import WarningMessage from '@common/components/WarningMessage';
import { useMethodologyContext } from '@admin/pages/methodology/contexts/MethodologyContext';
import React from 'react';
import { generatePath } from 'react-router';
import {
  publicationReleasesRoute,
  PublicationRouteParams,
} from '@admin/routes/publicationRoutes';
import Link from '@admin/components/Link';

const MethodologySummaryPage = () => {
  const { methodologyId, methodology } = useMethodologyContext();

  return (
    <>
      <h2>Methodology summary</h2>

      {methodology ? (
        <>
          <SummaryList>
            <SummaryListItem term="Title">{methodology.title}</SummaryListItem>
            <SummaryListItem term="Published on">
              {methodology.published ? (
                <FormattedDate>{methodology.published}</FormattedDate>
              ) : (
                'Not yet published'
              )}
            </SummaryListItem>
            <SummaryListItem term="Owning publication">
              <Link
                to={generatePath<PublicationRouteParams>(
                  publicationReleasesRoute.path,
                  { publicationId: methodology.owningPublication.id },
                )}
              >
                {methodology.owningPublication.title}
              </Link>
            </SummaryListItem>
            {methodology.otherPublications &&
              methodology.otherPublications.length > 0 && (
                <SummaryListItem term="Other publications">
                  <ul className="govuk-!-margin-top-0">
                    {methodology.otherPublications?.map(publication => (
                      <li
                        key={publication.id}
                        data-testid="other-publication-item"
                      >
                        <Link
                          to={`${generatePath<PublicationRouteParams>(
                            publicationReleasesRoute.path,
                            { publicationId: publication.id },
                          )}`}
                        >
                          {publication.title}
                        </Link>
                      </li>
                    ))}
                  </ul>
                </SummaryListItem>
              )}
          </SummaryList>

          {methodology.status !== 'Approved' && (
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
    </>
  );
};

export default MethodologySummaryPage;
