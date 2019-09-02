import ManageReleaseContext, {ManageRelease} from "@admin/pages/release/ManageReleaseContext";
import {getReleaseStatusLabel} from "@admin/pages/release/util/releaseSummaryUtil";
import commonService from '@admin/services/common/service';
import {dayMonthYearIsComplete, dayMonthYearToDate} from "@admin/services/common/types";
import {Comment} from "@admin/services/dashboard/types";
import releaseService from '@admin/services/release/edit-release/summary/service';
import {ReleaseSummaryDetails} from "@admin/services/release/types";
import FormFieldset from "@common/components/form/FormFieldset";
import FormRadioGroup from "@common/components/form/FormRadioGroup";
import FormattedDate from "@common/components/FormattedDate";
import classNames from "classnames";
import React, {useContext, useEffect, useState} from 'react';

type PageMode = 'edit' | 'preview';

interface ReleaseTypeIcon {
  url: string;
  altText: string;
}

const nationalStatisticsLogo: ReleaseTypeIcon = {
  url: '/static/images/UKSA-quality-mark.jpg',
  altText: 'UK statistics authority quality mark',
};

interface Model {
  unresolvedComments: Comment[];
  pageMode: PageMode;
  releaseSummary: ReleaseSummaryDetails;
  releaseTypeIcon?: ReleaseTypeIcon;
}

const ReleaseContentPage = () => {

  const [model, setModel] = useState<Model>();

  const { releaseId, publication } = useContext(ManageReleaseContext) as ManageRelease;

  useEffect(() => {

    Promise.all([
      commonService.getReleaseTypes(),
      releaseService.getReleaseSummaryDetails(releaseId),
    ]).then(([releaseTypes, releaseSummary]) => {

      const unresolvedComments: Comment[] = [{
        message: 'Please resolve this.\nThank you.',
        authorName: 'Amy Newton',
        createdDate: new Date('2019-08-10 10:15').toISOString(),
      }, {
        message: 'And this too.\nThank you.',
        authorName: 'Dave Matthews',
        createdDate: new Date('2019-06-13 10:15').toISOString(),
      }];

      const nationalStatisticsType = releaseTypes.find(type => type.title === 'National Statistics');

      setModel({
        unresolvedComments,
        pageMode: 'edit',
        releaseSummary,
        releaseTypeIcon: nationalStatisticsType && releaseSummary.typeId === nationalStatisticsType.id ? nationalStatisticsLogo : undefined,
      });

    });
  }, [releaseId]);

  return (
    <>
      {model && (
        <>
          <div className="govuk-form-group">
            {model.unresolvedComments.length > 0 && (
              <div className="govuk-warning-text">
                <span className="govuk-warning-text__icon" aria-hidden="true">
                  !
                </span>
                <strong className="govuk-warning-text__text">
                  <span className="govuk-warning-text__assistive">Warning</span>
                  There are {model.unresolvedComments.length} unresolved comments
                </strong>
              </div>
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
                <span className="govuk-caption-l">{model.releaseSummary.timePeriodCoverage.label} {model.releaseSummary.releaseName}</span>
                {publication.title}
              </h1>
              <div className="govuk-grid-row">
                <div className="govuk-grid-column-two-thirds">
                  <div className="govuk-grid-row">
                    <div className="govuk-grid-column-three-quarters">
                      <span className="govuk-tag">
                        {getReleaseStatusLabel(model.releaseSummary.status)}
                      </span>

                      <dl className="dfe-meta-content">
                        <dt className="govuk-caption-m">Publish date: </dt>
                        <dd>
                          <strong><FormattedDate>{model.releaseSummary.publishScheduled}</FormattedDate></strong>
                        </dd>
                        <div>
                          <dt className="govuk-caption-m">Next update: </dt>
                          <dd>
                            {dayMonthYearIsComplete(model.releaseSummary.nextReleaseDate) && (
                              <strong>
                                <FormattedDate>
                                  {dayMonthYearToDate(model.releaseSummary.nextReleaseDate)}
                                </FormattedDate>
                              </strong>
                            )}
                          </dd>
                        </div>
                      </dl>
                    </div>

                    {model.releaseTypeIcon && (
                      <div className="govuk-grid-column-one-quarter">
                        <img
                          src={model.releaseTypeIcon.url}
                          alt={model.releaseTypeIcon.altText}
                          height="120"
                          width="120"
                        />
                      </div>
                    )}
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
