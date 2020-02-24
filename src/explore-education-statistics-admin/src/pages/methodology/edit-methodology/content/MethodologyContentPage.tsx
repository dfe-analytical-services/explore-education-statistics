import Accordion from '@admin/components/EditableAccordion';
import AccordionSection from '@admin/components/EditableAccordionSection';
import PrintThisPage from '@admin/modules/find-statistics/components/PrintThisPage';
import methodologyService from '@admin/services/methodology/service';
import withErrorControl, {
  ErrorControlProps,
} from '@admin/validation/withErrorControl';
import { FormFieldset, FormRadioGroup } from '@common/components/form';
import FormattedDate from '@common/components/FormattedDate';
import PageSearchForm from '@common/components/PageSearchForm';
import { EditingContext } from '@common/modules/find-statistics/util/wrapEditableComponent';
import classNames from 'classnames';
import sortBy from 'lodash/sortBy';
import React, { useState } from 'react';
import { MethodologyTabProps } from '../MethodologyPage';

const MethodologyContentPage = ({
  methodology,
  refreshMethodology,
  handleApiErrors,
}: ErrorControlProps & MethodologyTabProps) => {
  const [isEditing, setIsEditing] = useState(true);

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
          isCommenting: false,
          isReviewing: false,
          releaseId: methodology.id,
          availableDataBlocks: [],
        }}
      >
        <div
          className={classNames('govuk-width-container', {
            'dfe-align--left-hand-controls': isEditing,
            'dfe-hide-controls': !isEditing,
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
                          <FormattedDate>{methodology.published}</FormattedDate>
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

            <Accordion
              id="contents-accordion"
              canReorder
              sectionName="Contents"
              onSaveOrder={async order => {
                await methodologyService.updateContentSectionsOrder(
                  methodology.id,
                  order,
                );
                refreshMethodology();
              }}
              onAddSection={async () => {
                await methodologyService.addContentSection(methodology.id, {
                  order: methodology.content.length,
                });
                refreshMethodology();
              }}
            >
              {sortBy(methodology.content, 'order').map((section, index) => (
                <AccordionSection
                  id={section.id}
                  key={section.id}
                  heading={section.heading}
                  index={index}
                  canEditHeading
                  onHeadingChange={async heading => {
                    await methodologyService.updateContentSectionHeading(
                      methodology.id,
                      section.id as string,
                      heading,
                    );
                    refreshMethodology();
                  }}
                  onRemoveSection={async () => {
                    await methodologyService.removeContentSection(
                      methodology.id,
                      section.id as string,
                    );
                    refreshMethodology();
                  }}
                />
              ))}
            </Accordion>
          </section>
        </div>
      </EditingContext.Provider>
    </>
  );
};

export default withErrorControl(MethodologyContentPage);
