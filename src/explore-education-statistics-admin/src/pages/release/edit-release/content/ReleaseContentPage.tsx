// eslint-disable-next-line import/no-named-as-default
import Link from '@admin/components/Link';
import EditableTextRenderer from '@admin/modules/find-statistics/components/EditableTextRenderer';
import ManageReleaseContext, {
  ManageRelease,
} from '@admin/pages/release/ManageReleaseContext';
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
import { AbstractRelease } from '@common/services/publicationService';
import classNames from 'classnames';
import React, { useContext, useEffect, useState } from 'react';
import PageSearchForm from '@common/components/PageSearchForm';
import PublicationReleaseContent, {
  ComponentTypes,
} from '@admin/modules/find-statistics/PublicationReleaseContent';
import ReactMarkdown from 'react-markdown';
import EditableMarkdownRenderer from '@admin/modules/find-statistics/components/EditableMarkdownRenderer';
import { EditableContentBlock } from '@admin/services/publicationService';
import EditableContentBlockComponent from '@admin/modules/find-statistics/components/EditableContentBlock';
import DataBlock from '@common/modules/find-statistics/components/DataBlock';
import ContentBlock from '@common/modules/find-statistics/components/ContentBlock';
import EditableDataBlock from '@admin/modules/find-statistics/components/EditableDataBlock';
import EditableAccordion from '@admin/components/EditableAccordion';
import EditableAccordionSection from '@admin/components/EditableAccordionSection';
import PrintThisPage from '../../../../modules/find-statistics/components/PrintThisPage';

type PageMode = 'edit' | 'preview';

interface Model {
  unresolvedComments: Comment[];
  pageMode: PageMode;
  releaseSummary: ReleaseSummaryDetails;
  theme: IdTitlePair;
  release: AbstractRelease<EditableContentBlock>;
}

type PageModeComponentTypesType = {
  [key in PageMode]: ComponentTypes;
};

const PageModeComponentTypes: PageModeComponentTypesType = {
  edit: {
    TextRenderer: EditableTextRenderer,
    MarkdownRenderer: EditableMarkdownRenderer,
    Link,
    PrintThisPage,
    SearchForm: PageSearchForm,
    Accordion: EditableAccordion,
    AccordionSection: EditableAccordionSection,
    ContentBlock: EditableContentBlockComponent,
    DataBlock: EditableDataBlock,
  },
  preview: {
    TextRenderer: EditableTextRenderer,
    MarkdownRenderer: ReactMarkdown,
    Link,
    PrintThisPage,
    SearchForm: PageSearchForm,
    Accordion,
    AccordionSection,
    ContentBlock,
    DataBlock,
  },
};

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
      // <editor-fold desc="TODO - content population">

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

      const contentBlock = {
        order: 0,
        heading: 'test',
        caption: 'test',
        content: [
          {
            type: 'HtmlBlock',
            body: 'This is a test',
            comments: [
              {
                name: 'A user',
                time: new Date(),
                comment: 'A comment',
                state: 'open',
              },
            ],
          },
        ],
      };

      const releaseDataAsEditable = {
        ...releaseData,
        keyStatistics: releaseData.keyStatistics as EditableContentBlock,
        content: (releaseData.content &&
          releaseData.content.map(section => ({
            ...section,
            content: section.content.map<EditableContentBlock>(block => ({
              ...block,
              comments: [],
            })),
          }))) || [contentBlock, contentBlock],
      };

      const release: AbstractRelease<EditableContentBlock> = {
        ...releaseDataAsEditable,
        summary: 'This is the summary ..... ',
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
            theme,
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

      // </editor-fold>

      setModel({
        unresolvedComments,
        pageMode: 'edit',
        releaseSummary,
        theme,
        release,
      });
    });
  }, [releaseId, publication.themeId, publication]);

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
              <PublicationReleaseContent
                basicPublication={publication}
                release={model.release}
                releaseSummary={model.releaseSummary}
                styles={{}}
                {...PageModeComponentTypes[model.pageMode]}
              />
            </div>
          </div>
        </>
      )}
    </>
  );
};

export default ReleaseContentPage;
