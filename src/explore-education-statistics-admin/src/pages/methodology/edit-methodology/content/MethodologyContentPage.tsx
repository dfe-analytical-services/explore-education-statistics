import PrintThisPage from '@admin/modules/find-statistics/components/PrintThisPage';
import { FormFieldset, FormRadioGroup } from '@common/components/form';
import FormattedDate from '@common/components/FormattedDate';
import LoadingSpinner from '@common/components/LoadingSpinner';
import PageSearchForm from '@common/components/PageSearchForm';
import { EditingContext } from '@common/modules/find-statistics/util/wrapEditableComponent';
import classNames from 'classnames';
import React, { useState } from 'react';
import MethodologyAccordion from './components/MethodologyAccordion';
import { useMethodologyState } from './context/MethodologyContext';

const MethodologyContentPage = () => {
  const [isEditing, setIsEditing] = useState(true);

  const { methodology } = useMethodologyState();

  if (methodology)
    return (
      <>
        <FormFieldset
          id="pageModelFieldset"
          legend=""
          className="dfe-toggle-edit"
          legendHidden
        >
          <FormRadioGroup
            id="pageMode"
            name="pageMode"
            value={isEditing ? 'edit' : 'preview'}
            legend="Set page view"
            small
            options={[
              {
                label: 'Add / edit methodology content',
                value: 'edit',
              },
              {
                label: 'Preview content',
                value: 'preview',
              },
            ]}
            onChange={event => {
              setIsEditing(event.target.value === 'edit');
            }}
          />
        </FormFieldset>
        <EditingContext.Provider
          value={{
            isEditing,
          }}
        >
          <div
            className={classNames('govuk-width-container', {
              'dfe-align--comments': isEditing,
              'dfe-hide-comments': !isEditing,
            })}
          >
            <section
              className={isEditing ? 'dfe-page-editing' : 'dfe-page-preview'}
            >
              <h1
                className="govuk-heading-xl"
                data-testid={`page-title ${methodology.title}`}
              >
                {methodology.title}
              </h1>

              <div className="govuk-grid-row">
                <div className="govuk-grid-column-two-thirds">
                  <dl className="dfe-meta-content govuk-!-margin-top-0">
                    <div>
                      <dt className="govuk-caption-m">Published: </dt>
                      <dd data-testid="published-date">
                        <strong>
                          {methodology.published ? (
                            <FormattedDate>
                              {methodology.published}
                            </FormattedDate>
                          ) : (
                            'Not yet published'
                          )}
                        </strong>
                      </dd>
                    </div>
                    {methodology.lastUpdated &&
                      methodology.lastUpdated.length > 0 && (
                        <>
                          <dt className="govuk-caption-m">Last updated: </dt>
                          <dd data-testid="last-updated">
                            <strong>
                              <FormattedDate>
                                {methodology.lastUpdated}
                              </FormattedDate>
                            </strong>
                          </dd>
                        </>
                      )}
                  </dl>
                  {!isEditing && (
                    <>
                      <PrintThisPage
                        analytics={{
                          category: 'Page print',
                          action: 'Print this page link selected',
                        }}
                      />
                      <PageSearchForm inputLabel="Search in this methodology page." />
                    </>
                  )}
                </div>
              </div>
              <MethodologyAccordion
                methodology={methodology}
                sectionKey="content"
              />
              {!isEditing && methodology.annexes.length ? (
                <h2>Annexes</h2>
              ) : null}
              <MethodologyAccordion
                methodology={methodology}
                sectionKey="annexes"
              />
            </section>
          </div>
        </EditingContext.Provider>
      </>
    );
  return <LoadingSpinner />;
};

export default MethodologyContentPage;
