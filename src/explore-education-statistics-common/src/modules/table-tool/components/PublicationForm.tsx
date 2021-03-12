import Details from '@common/components/Details';
import DetailsMenu from '@common/components/DetailsMenu';
import {
  Form,
  FormFieldRadioGroup,
  FormFieldset,
  FormGroup,
  FormTextSearchInput,
} from '@common/components/form';
import SummaryList from '@common/components/SummaryList';
import SummaryListItem from '@common/components/SummaryListItem';
import { Theme } from '@common/services/tableBuilderService';
import createErrorHelper from '@common/validation/createErrorHelper';
import Yup from '@common/validation/yup';
import { Formik } from 'formik';
import React, { useState } from 'react';
import { InjectedWizardProps } from './Wizard';
import WizardStepFormActions from './WizardStepFormActions';
import WizardStepHeading from './WizardStepHeading';

export interface PublicationFormValues {
  publicationId: string;
}

export type PublicationFormSubmitHandler = (
  values: PublicationFormValues,
) => void;

interface Props {
  initialValues?: PublicationFormValues;
  onSubmit: PublicationFormSubmitHandler;
  options: Theme[];
}

const formId = 'publicationForm';

const PublicationForm = (props: Props & InjectedWizardProps) => {
  const {
    options,
    onSubmit,
    isActive,
    goToNextStep,
    initialValues = {
      publicationId: '',
    },
  } = props;

  const [searchTerm, setSearchTerm] = useState('');
  const lowercaseSearchTerm = searchTerm.toLowerCase();

  const stepHeading = (
    <WizardStepHeading {...props} fieldsetHeading>
      Choose a publication
    </WizardStepHeading>
  );

  return (
    <Formik<PublicationFormValues>
      enableReinitialize
      initialValues={initialValues}
      validateOnBlur={false}
      validateOnChange={false}
      validationSchema={Yup.object<PublicationFormValues>({
        publicationId: Yup.string().required('Choose publication'),
      })}
      onSubmit={async values => {
        await onSubmit(values);
        goToNextStep();
      }}
    >
      {form => {
        const { values } = form;
        const { getError } = createErrorHelper(form);

        const filteredOptions: Theme[] = options
          .filter(theme =>
            theme.topics.some(topic =>
              topic.publications.some(
                publication =>
                  publication.id === values.publicationId ||
                  publication.title.toLowerCase().includes(lowercaseSearchTerm),
              ),
            ),
          )
          .map(group => ({
            ...group,
            topics: group.topics
              .filter(topic =>
                topic.publications.some(
                  publication =>
                    publication.id === values.publicationId ||
                    publication.title
                      .toLowerCase()
                      .includes(lowercaseSearchTerm),
                ),
              )
              .map(topic => ({
                ...topic,
                publications: topic.publications.filter(
                  publication =>
                    publication.id === values.publicationId ||
                    publication.title
                      .toLowerCase()
                      .includes(lowercaseSearchTerm),
                ),
              })),
          }));

        if (isActive) {
          return (
            <Form {...form} id={formId} showSubmitError>
              <FormFieldset
                error={getError('publicationId')}
                id="publicationId"
                legend={stepHeading}
              >
                <FormGroup>
                  <FormTextSearchInput
                    id={`${formId}-publicationIdSearch`}
                    label="Search publications"
                    name="publicationSearch"
                    onChange={event => setSearchTerm(event.target.value)}
                    onKeyPress={event => {
                      if (event.key === 'Enter') {
                        event.preventDefault();
                      }
                    }}
                    width={20}
                  />
                </FormGroup>

                <FormGroup>
                  <div aria-live="assertive">
                    {filteredOptions.length > 0 ? (
                      filteredOptions.map(theme => (
                        <DetailsMenu
                          jsRequired
                          summary={theme.title}
                          key={theme.id}
                          id={`${formId}-theme-${theme.id}`}
                          open={
                            searchTerm !== '' ||
                            theme.topics.some(topic =>
                              topic.publications.some(
                                publication =>
                                  publication.id === values.publicationId,
                              ),
                            )
                          }
                        >
                          {theme.topics.map(topic => (
                            <DetailsMenu
                              summary={topic.title}
                              key={topic.id}
                              id={`${formId}-topic-${topic.id}`}
                              open={
                                searchTerm !== '' ||
                                topic.publications.some(
                                  publication =>
                                    publication.id === values.publicationId,
                                )
                              }
                            >
                              <FormFieldRadioGroup
                                legend={`Choose option from ${topic.title}`}
                                legendHidden
                                small
                                showError={false}
                                name="publicationId"
                                disabled={form.isSubmitting}
                                options={topic.publications.map(
                                  publication => ({
                                    label: publication.title,
                                    value: publication.id,
                                  }),
                                )}
                              />
                            </DetailsMenu>
                          ))}
                        </DetailsMenu>
                      ))
                    ) : (
                      <p>No publications found</p>
                    )}
                  </div>
                </FormGroup>
              </FormFieldset>

              <WizardStepFormActions {...props} />
            </Form>
          );
        }

        const publication = options
          .flatMap(option => option.topics)
          .flatMap(option => option.publications)
          .find(option => option.id === form.values.publicationId);

        return (
          <>
            <div className="govuk-grid-row">
              <div className="govuk-grid-column-one-quarter">{stepHeading}</div>
              <div className="govuk-grid-column-three-quarters">
                <Details summary="View details">
                  <SummaryList noBorder>
                    <SummaryListItem term="Publication">
                      {publication?.title}
                    </SummaryListItem>
                  </SummaryList>
                </Details>
              </div>
            </div>
          </>
        );
      }}
    </Formik>
  );
};

export default PublicationForm;
