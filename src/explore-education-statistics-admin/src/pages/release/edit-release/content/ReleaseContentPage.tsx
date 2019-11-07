import Link from '@admin/components/Link';
import BasicReleaseSummary from '@admin/pages/release/edit-release/content/components/BasicReleaseSummary';
import EditableTextRenderer from '@admin/pages/release/edit-release/content/components/EditableTextRenderer';
import ManageReleaseContext, { ManageRelease } from '@admin/pages/release/ManageReleaseContext';
import { getTimePeriodCoverageDateRangeStringShort } from '@admin/pages/release/util/releaseSummaryUtil';
import commonService from '@admin/services/common/service';
import { IdTitlePair } from '@admin/services/common/types';
import { Comment } from '@admin/services/dashboard/types';
import releaseContentService from '@admin/services/release/edit-release/content/service';
import releaseSummaryService from '@admin/services/release/edit-release/summary/service';
import { ReleaseSummaryDetails } from '@admin/services/release/types';
import Accordion from '@common/components/Accordion';
import AccordionSection from '@common/components/AccordionSection';
import FormFieldset from '@common/components/form/FormFieldset';
import FormRadioGroup from '@common/components/form/FormRadioGroup';
import WarningMessage from '@common/components/WarningMessage';
import { AbstractRelease, ContentBlock, ReleaseType } from '@common/services/publicationService';
import classNames from 'classnames';
import React, { useContext, useEffect, useState } from 'react';
import PageSearchForm from '@common/components/PageSearchForm';
import PublicationReleaseContent from '@common/modules/find-statistics/PublicationReleaseContent';
import EditableMarkdownRenderer from './components/EditableMarkdownRenderer';
import PrintThisPage from './components/PrintThisPage';

type PageMode = 'edit' | 'preview';

interface Model {
  unresolvedComments: Comment[];
  pageMode: PageMode;
  releaseSummary: ReleaseSummaryDetails;
  theme: IdTitlePair;
  release: AbstractRelease<ContentBlock>;
}

const ReleaseContentPage = () => {
  const [model, setModel] = useState<Model>();

  const { releaseId, publication } = useContext(
    ManageReleaseContext,
  ) as ManageRelease;

  useEffect(() => {
    Promise.all([
      releaseSummaryService.getReleaseSummaryDetails(releaseId),
      commonService.getBasicThemeDetails(publication.themeId),
      releaseContentService.getRelease(releaseId),
    ]).then(([releaseSummary, theme, releaseData]) => {
      const unresolvedComments: Comment[] = [
        {
          message: 'Please resolve this.\nThank you.',
          authorName: 'Amy Newton',
          createdDate: new Date('2019-08-10 10:15').toISOString(),
        },
        {
          message: 'And this too.\nThank you.',
          authorName: 'Dave Matthews',
          createdDate: new Date('2019-06-13 10:15').toISOString(),
        },
      ];

      const release: AbstractRelease<ContentBlock> = {
        ...releaseData,
        updates: [
          {
            id: '',
            on: '',
            reason: '',
            releaseId: '',
          },
        ],
        publication: {
          ...publication,
          slug: '',
          description: '',
          dataSource: '',
          summary: '',
          releases: [],
          legacyReleases: [],
          topic: {
            theme: {
              title: '',
            },
          },
          nextUpdate: '',
          contact: {
            contactName: '',
            contactTelNo: '',
            teamEmail: '',
            teamName: '',
          },
        },
      };

      setModel({
        unresolvedComments,
        pageMode: 'edit',
        releaseSummary,
        theme,
        release,
      });
    });


  }, [releaseId, publication.themeId]);

  return (
    <>
      {model && (
        <>
          <div className="govuk-form-group">
            {model.unresolvedComments.length > 0 && (
              <WarningMessage>
                There are {model.unresolvedComments.length} unresolved comments
              </WarningMessage>
            )}

            <FormFieldset
              id="pageModelFieldset"
              legend=""
              className="dfe-toggle-edit"
              legendHidden
            >
              <FormRadioGroup
                id="pageMode"
                name="pageMode"
                value={model.pageMode}
                legend="Set page view"
                options={[
                  {
                    label: 'Add / view comments and edit content',
                    value: 'edit',
                  },
                  {
                    label: 'Preview content',
                    value: 'preview',
                  },
                ]}
                inline
                onChange={event => {
                  setModel({
                    ...model,
                    pageMode: event.target.value as PageMode,
                  });
                }}
              />
            </FormFieldset>
          </div>
          <div
            className={classNames('govuk-width-container', {
              'dfe-align--comments': model.pageMode === 'edit',
              'dfe-hide-comments': model.pageMode === 'preview',
            })}
          >
            <div className={model.pageMode === 'edit' ? 'page-editing' : ''}>
              <h1 className="govuk-heading-l">
                <span className="govuk-caption-l">
                  {model.releaseSummary.timePeriodCoverage.label}{' '}
                  {getTimePeriodCoverageDateRangeStringShort(
                    model.releaseSummary.releaseName,
                    '/',
                  )}
                </span>
                {publication.title}
              </h1>
              <div className="govuk-grid-row">
                <div className="govuk-grid-column-two-thirds">
                  <div className="govuk-grid-row">
                    <BasicReleaseSummary release={model.releaseSummary} />
                  </div>
                </div>
              </div>
              <hr />
              <h2
                className="govuk-heading-m govuk-!-margin-top-9"
                data-testid="extra-information"
              >
                Help and support
              </h2>
              <Accordion
                // publicationTitle={publication.title}
                id="static-content-section"
              >
                <AccordionSection
                  heading={`${publication.title}: methodology`}
                  caption="Find out how and why we collect, process and publish these statistics"
                  headingTag="h3"
                >
                  <p>
                    Read our{' '}
                    <Link to={`/methodology/${publication.methodologyId}`}>
                      {`${publication.title}: methodology`}
                    </Link>{' '}
                    guidance.
                  </p>
                </AccordionSection>
                {model.releaseSummary.type &&
                model.releaseSummary.type.title ===
                ReleaseType.NationalStatistics && (
                  <AccordionSection
                    heading="National Statistics"
                    headingTag="h3"
                  >
                    <p className="govuk-body">
                      The{' '}
                      <a href="https://www.statisticsauthority.gov.uk/">
                        United Kingdom Statistics Authority
                      </a>{' '}
                      designated these statistics as National Statistics in
                      accordance with the{' '}
                      <a href="https://www.legislation.gov.uk/ukpga/2007/18/contents">
                        Statistics and Registration Service Act 2007
                      </a>{' '}
                      and signifying compliance with the Code of Practice for
                      Statistics.
                    </p>
                    <p className="govuk-body">
                      Designation signifying their compliance with the
                      authority's{' '}
                      <a href="https://www.statisticsauthority.gov.uk/code-of-practice/the-code/">
                        Code of Practice for Statistics
                      </a>{' '}
                      which broadly means these statistics are:
                    </p>
                    <ul className="govuk-list govuk-list--bullet">
                      <li>
                        managed impartially and objectively in the public
                        interest
                      </li>
                      <li>meet identified user needs</li>
                      <li>produced according to sound methods</li>
                      <li>well explained and readily accessible</li>
                    </ul>
                    <p className="govuk-body">
                      Once designated as National Statistics it's a statutory
                      requirement for statistics to follow and comply with the
                      Code of Practice for Statistics to be observed.
                    </p>
                    <p className="govuk-body">
                      Find out more about the standards we follow to produce
                      these statistics through our{' '}
                      <a href="https://www.gov.uk/government/publications/standards-for-official-statistics-published-by-the-department-for-education">
                        Standards for official statistics published by DfE
                      </a>{' '}
                      guidance.
                    </p>
                  </AccordionSection>
                )}
                <AccordionSection heading="Contact us" headingTag="h3">
                  <p>
                    If you have a specific enquiry about {model.theme.title}{' '}
                    statistics and data:
                  </p>
                  <h4 className="govuk-heading-s govuk-!-margin-bottom-0">
                    {publication.contact && publication.contact.teamName}
                  </h4>
                  <p className="govuk-!-margin-top-0">
                    Email <br />
                    {publication.contact && (
                      <a href={`mailto:${publication.contact.teamEmail}`}>
                        {publication.contact.teamEmail}
                      </a>
                    )}
                  </p>
                  <p>
                    {publication.contact && (
                      <>
                        Telephone: {publication.contact.contactName} <br />{' '}
                        {publication.contact.contactTelNo}
                      </>
                    )}
                  </p>
                  <h4 className="govuk-heading-s govuk-!-margin-bottom-0">
                    Press office
                  </h4>
                  <p className="govuk-!-margin-top-0">
                    If you have a media enquiry:
                  </p>
                  <p>
                    Telephone <br />
                    020 7925 6789
                  </p>
                  <h4 className="govuk-heading-s govuk-!-margin-bottom-0">
                    Public enquiries
                  </h4>
                  <p className="govuk-!-margin-top-0">
                    If you have a general enquiry about the Department for
                    Education (DfE) or education:
                  </p>
                  <p>
                    Telephone <br />
                    037 0000 2288
                  </p>
                </AccordionSection>
              </Accordion>`
            </div>

            <PublicationReleaseContent
              data={model.release}
              styles={{}}
              TextRenderer={EditableTextRenderer}
              MarkdownRenderer={EditableMarkdownRenderer}
              Link={Link}
              PrintThisPage={PrintThisPage}
              SearchForm={PageSearchForm}
            />
          </div>
        </>
      )}
    </>
  );
};

export default ReleaseContentPage;
