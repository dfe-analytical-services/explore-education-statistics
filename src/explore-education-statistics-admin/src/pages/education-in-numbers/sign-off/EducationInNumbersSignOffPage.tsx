import React from 'react';
import { generatePath } from 'react-router';
import { useHistory } from 'react-router-dom';
import { useEducationInNumbersPageContext } from '@admin/pages/education-in-numbers/contexts/EducationInNumbersContext';
import {
  EducationInNumbersRouteParams,
  educationInNumbersSummaryRoute,
} from '@admin/routes/educationInNumbersRoutes';
import WarningMessage from '@common/components/WarningMessage';
import FormattedDate from '@common/components/FormattedDate';
import SummaryList from '@common/components/SummaryList';
import SummaryListItem from '@common/components/SummaryListItem';
import Button from '@common/components/Button';
import educationInNumbersService from '@admin/services/educationInNumbersService';

const EducationInNumbersSignOffPage = () => {
  const { educationInNumbersPage, onEducationInNumbersPageChange } =
    useEducationInNumbersPageContext();

  const history = useHistory();

  const isEditable = educationInNumbersPage.published === undefined;

  // @MarkFix add link to its public page here? or maybe elsewhere

  return (
    <>
      <h2>Sign off</h2>

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
            <SummaryListItem term="Published on">
              {educationInNumbersPage.published ? (
                // @MarkFix it's utc time when it should be gmt
                <FormattedDate format="HH:mm:ss - d MMMM yyyy">
                  {educationInNumbersPage.published}
                </FormattedDate>
              ) : (
                'Not yet published'
              )}
            </SummaryListItem>
          </SummaryList>

          {isEditable && (
            <Button
              onClick={async () => {
                // @MarkFix add "are you sure you want to publish?" modal?
                const publishedPage =
                  await educationInNumbersService.publishEducationInNumbersPage(
                    educationInNumbersPage.id,
                  );

                onEducationInNumbersPageChange(publishedPage);

                history.push(
                  generatePath<EducationInNumbersRouteParams>(
                    educationInNumbersSummaryRoute.path,
                    {
                      educationInNumbersPageId: publishedPage.id,
                    },
                  ),
                );
              }}
            >
              Publish
            </Button>
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

export default EducationInNumbersSignOffPage;
