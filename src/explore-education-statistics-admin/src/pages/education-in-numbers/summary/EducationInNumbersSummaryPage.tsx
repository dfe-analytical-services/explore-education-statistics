import React from 'react';
import { generatePath } from 'react-router';
import { useEducationInNumbersPageContext } from '@admin/pages/education-in-numbers/contexts/EducationInNumbersContext';
import {
  educationInNumbersSummaryEditRoute,
  EducationInNumbersRouteParams,
} from '@admin/routes/educationInNumbersRoutes';
import WarningMessage from '@common/components/WarningMessage';
import ButtonLink from '@admin/components/ButtonLink';
import FormattedDate from '@common/components/FormattedDate';
import SummaryList from '@common/components/SummaryList';
import SummaryListItem from '@common/components/SummaryListItem';
import { GetEducationInNumbersPageStatus } from '@admin/pages/education-in-numbers/EducationInNumbersListPage';

const EducationInNumbersSummaryPage = () => {
  const { educationInNumbersPage: page } = useEducationInNumbersPageContext();

  const isEditable = page.published === undefined && page.version === 0; // editing amendments is not allowed because no redirects

  return (
    <>
      <h2>Page summary</h2>

      {page ? (
        <>
          <SummaryList>
            <SummaryListItem term="Title">{page.title}</SummaryListItem>
            <SummaryListItem term="Slug">{page.slug ?? 'N/A'}</SummaryListItem>
            <SummaryListItem term="Description">
              {page.description}
            </SummaryListItem>
            <SummaryListItem term="Status">
              {GetEducationInNumbersPageStatus(page)}
            </SummaryListItem>
            <SummaryListItem term="Published on">
              {page.published ? (
                // @MarkFix it's utc when it should be gmt
                <FormattedDate format="HH:mm:ss - d MMMM yyyy">
                  {page.published}
                </FormattedDate>
              ) : (
                'Not yet published'
              )}
            </SummaryListItem>
          </SummaryList>

          {isEditable && (
            <ButtonLink
              to={generatePath<EducationInNumbersRouteParams>(
                educationInNumbersSummaryEditRoute.path,
                {
                  educationInNumbersPageId: page.id,
                },
              )}
            >
              Edit summary
            </ButtonLink>
          )}
        </>
      ) : (
        <WarningMessage>
          There was a problem loading the page summary.
        </WarningMessage>
      )}
    </>
  );
};

export default EducationInNumbersSummaryPage;
