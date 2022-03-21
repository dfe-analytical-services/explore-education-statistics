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
import WizardStepSummary from '@common/modules/table-tool/components/WizardStepSummary';
import { PublicationSummary, Theme } from '@common/services/themeService';
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

export type PublicationFormSubmitHandler = (values: {
  publication: PublicationSummary;
}) => void;

const formId = 'publicationForm';

interface Props extends InjectedWizardProps {
  initialValues?: PublicationFormValues;
  options: Theme[];
  onSubmit: PublicationFormSubmitHandler;
}

const PublicationForm = ({
  initialValues = {
    publicationId: '',
  },
  options,
  onSubmit,
  ...stepProps
}: Props) => {
  const { isActive, goToNextStep } = stepProps;

  const [searchTerm, setSearchTerm] = useState('');
  const lowercaseSearchTerm = searchTerm.toLowerCase();

  const stepHeading = (
    <WizardStepHeading {...stepProps} fieldsetHeading>
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
      onSubmit={async ({ publicationId }) => {
        const publications = options.flatMap(theme =>
          theme.topics.flatMap(topic => topic.publications),
        );
        const publication = publications.find(p => p.id === publicationId);

        if (!publication) {
          throw new Error('Selected publication not found');
        }

        await goToNextStep(async () => {
          await onSubmit({ publication });
        });
      }}
    >
      {form => {
        const { values } = form;
        const { getError } = createErrorHelper(form);

        const filteredOptions = options
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
                          detailsId="theme"
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
                              detailsId="topic"
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

              <WizardStepFormActions {...stepProps} />
            </Form>
          );
        }

        const publication = options
          .flatMap(option => option.topics)
          .flatMap(option => option.publications)
          .find(option => option.id === form.values.publicationId);

        return (
          <WizardStepSummary {...stepProps} goToButtonText="Change publication">
            {stepHeading}

            <SummaryList noBorder>
              <SummaryListItem term="Publication">
                {publication?.title}
              </SummaryListItem>
            </SummaryList>
          </WizardStepSummary>
        );
      }}
    </Formik>
  );
};

export default PublicationForm;
