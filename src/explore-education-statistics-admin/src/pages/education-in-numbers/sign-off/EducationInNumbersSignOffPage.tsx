import React from 'react';
import { generatePath } from 'react-router';
import { useHistory } from 'react-router-dom';
import { useEducationInNumbersPageContext } from '@admin/pages/education-in-numbers/contexts/EducationInNumbersContext';
import {
  EducationInNumbersRouteParams,
  educationInNumbersSummaryRoute,
} from '@admin/routes/educationInNumbersRoutes';
import WarningMessage from '@common/components/WarningMessage';
import { formatInTimeZone } from 'date-fns-tz';
import SummaryList from '@common/components/SummaryList';
import SummaryListItem from '@common/components/SummaryListItem';
import Button from '@common/components/Button';
import educationInNumbersService from '@admin/services/educationInNumbersService';
import ModalConfirm from '@common/components/ModalConfirm';
import UrlContainer from '@common/components/UrlContainer';
import { useConfig } from '@admin/contexts/ConfigContext';

const EducationInNumbersSignOffPage = () => {
  const { educationInNumbersPage: page, onEducationInNumbersPageChange } =
    useEducationInNumbersPageContext();

  const history = useHistory();

  const { publicAppUrl } = useConfig();

  const isEditable = page.published === undefined;

  return (
    <>
      <h2>Sign off</h2>

      <p>The page will be accessible at:</p>

      <UrlContainer
        className="govuk-!-margin-bottom-4"
        id="public-page-url"
        url={`${publicAppUrl}/education-in-numbers/${
          page.slug !== undefined ? page.slug : ''
        }`}
      />

      {page ? (
        <>
          <SummaryList testId="page-list">
            <SummaryListItem term="Title">{page.title}</SummaryListItem>
            <SummaryListItem term="Slug">{page.slug ?? 'N/A'}</SummaryListItem>
            <SummaryListItem term="Description">
              {page.description}
            </SummaryListItem>
            <SummaryListItem term="Published on">
              {page.published
                ? formatInTimeZone(
                    page.published,
                    'Europe/London',
                    'HH:mm:ss - d MMMM yyyy',
                  )
                : 'Not yet published'}
            </SummaryListItem>
          </SummaryList>

          {isEditable && (
            <ModalConfirm
              title={`Are you sure you want to publish ${page.title}?`}
              triggerButton={<Button>Publish</Button>}
              onConfirm={async () => {
                const publishedPage =
                  await educationInNumbersService.publishEducationInNumbersPage(
                    page.id,
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
              <p>
                Once published, this Education In Numbers page will be live
                immediately.
              </p>
            </ModalConfirm>
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
