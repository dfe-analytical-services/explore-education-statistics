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
  const { educationInNumbersPageId, educationInNumbersPage } =
    useEducationInNumbersPageContext();

  const isEditable = educationInNumbersPage.published !== undefined;

  return (
    <>
      <h2>Page summary</h2>

      {educationInNumbersPage ? (
        <>
          <SummaryList>
            <SummaryListItem term="Title">
              {educationInNumbersPage.title}
            </SummaryListItem>
            <SummaryListItem term="Slug">
              {educationInNumbersPage.slug ?? 'N/A'}
            </SummaryListItem>
            <SummaryListItem term="Description">
              {educationInNumbersPage.description}
            </SummaryListItem>
            <SummaryListItem term="Status">
              {GetEducationInNumbersPageStatus(educationInNumbersPage)}
            </SummaryListItem>
            <SummaryListItem term="Published on">
              {educationInNumbersPage.published ? (
                // @MarkFix it's utc when it should be gmt
                <FormattedDate format="HH:mm:ss - d MMMM yyyy">
                  {educationInNumbersPage.published}
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
                  educationInNumbersPageId,
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
