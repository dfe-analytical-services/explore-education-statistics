import BasicReleaseSummary from "@admin/pages/release/edit-release/content/components/BasicReleaseSummary";
import ManageReleaseContext, {ManageRelease} from "@admin/pages/release/ManageReleaseContext";
import {
  getTimePeriodCoverageDateRangeStringShort,
} from "@admin/pages/release/util/releaseSummaryUtil";
import {Comment} from "@admin/services/dashboard/types";
import releaseService from '@admin/services/release/edit-release/summary/service';
import {ReleaseSummaryDetails} from "@admin/services/release/types";
import FormFieldset from "@common/components/form/FormFieldset";
import FormRadioGroup from "@common/components/form/FormRadioGroup";
import WarningMessage from "@common/components/WarningMessage";
import classNames from "classnames";
import React, {useContext, useEffect, useState} from 'react';

type PageMode = 'edit' | 'preview';

interface Model {
  unresolvedComments: Comment[];
  pageMode: PageMode;
  releaseSummary: ReleaseSummaryDetails;
}

const ReleaseContentPage = () => {

  const [model, setModel] = useState<Model>();

  const { releaseId, publication } = useContext(ManageReleaseContext) as ManageRelease;

  useEffect(() => {

    releaseService.getReleaseSummaryDetails(releaseId).then(releaseSummary => {

      const unresolvedComments: Comment[] = [{
        message: 'Please resolve this.\nThank you.',
        authorName: 'Amy Newton',
        createdDate: new Date('2019-08-10 10:15').toISOString(),
      }, {
        message: 'And this too.\nThank you.',
        authorName: 'Dave Matthews',
        createdDate: new Date('2019-06-13 10:15').toISOString(),
      }];

      setModel({
        unresolvedComments,
        pageMode: 'edit',
        releaseSummary,
      });
    });
  }, [releaseId]);

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
              id='pageModelFieldset'
              legend=''
              className='dfe-toggle-edit'
              legendHidden
            >
              <FormRadioGroup
                id='pageMode'
                name='pageMode'
                value={model.pageMode}
                legend='Set page view'
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
                  {getTimePeriodCoverageDateRangeStringShort(model.releaseSummary.releaseName, '/')}
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
            </div>
          </div>
        </>
      )}
    </>
  )
};

export default ReleaseContentPage;
