import {
  FormFieldset,
  FormGroup,
  FormRadioGroup,
} from '@common/components/form';
import SubmitError from '@common/components/form/util/SubmitError';
import SummaryList from '@common/components/SummaryList';
import SummaryListItem from '@common/components/SummaryListItem';
import WizardStepSummary from '@common/modules/table-tool/components/WizardStepSummary';
import {
  PublicationTreeSummary,
  Theme,
} from '@common/services/publicationService';
import Yup from '@common/validation/yup';
import { InjectedWizardProps } from '@common/modules/table-tool/components/Wizard';
import WizardStepHeading from '@common/modules/table-tool/components/WizardStepHeading';
import Button from '@common/components/Button';
import styles from '@admin/prototypes/components/PrototypePublicationForm.module.scss';
import PrototypeFormTextSearchInput from '@admin/prototypes/components/PrototypeFormTextSearchInput';
import orderBy from 'lodash/orderBy';
import React, { useMemo, useState } from 'react';
import FormProvider from '@common/components/form/rhf/FormProvider';
import RHFForm from '@common/components/form/rhf/RHFForm';
import RHFFormFieldRadioGroup from '@common/components/form/rhf/RHFFormFieldRadioGroup';

export interface PublicationFormValues {
  publicationId: string;
}

export type PublicationFormSubmitHandler = (values: {
  publication: PublicationTreeSummary;
}) => void;

const formId = 'publicationForm';

interface Props extends InjectedWizardProps {
  initialValues?: PublicationFormValues;
  options: Theme[];
  onSubmit: PublicationFormSubmitHandler;
}

const PrototypePublicationForm = ({
  initialValues = {
    publicationId: '',
  },
  options,
  onSubmit,
  ...stepProps
}: Props) => {
  const { isActive, goToNextStep } = stepProps;

  const [searchTerm, setSearchTerm] = useState('');
  const [selectedThemeId, setSelectedThemeId] = useState<string>('');

  const publications = useMemo(() => {
    if (selectedThemeId) {
      setSearchTerm('');
      return (
        options
          .find(option => option.id === selectedThemeId)
          ?.topics.flatMap(topic => topic.publications) ?? []
      );
    }
    if (searchTerm) {
      setSelectedThemeId('');
      return options
        .flatMap(theme => theme.topics.flatMap(topic => topic.publications))
        .filter(publication =>
          publication.title.toLowerCase().includes(searchTerm.toLowerCase()),
        );
    }

    return [];
  }, [options, searchTerm, selectedThemeId]);

  const stepHeading = (
    <WizardStepHeading {...stepProps} fieldsetHeading>
      Choose a publication
    </WizardStepHeading>
  );

  const handleSubmit = async ({ publicationId }: PublicationFormValues) => {
    const publication = publications.find(p => p.id === publicationId);

    if (!publication) {
      throw new SubmitError('Selected publication not found');
    }

    await goToNextStep(async () => {
      await onSubmit({ publication });
    });
  };

  return (
    <FormProvider
      enableReinitialize
      initialValues={initialValues}
      validationSchema={Yup.object<PublicationFormValues>({
        publicationId: Yup.string().required('Choose publication'),
      })}
    >
      {({ watch }) => {
        const values = watch();
        if (isActive) {
          return (
            <RHFForm id={formId} onSubmit={handleSubmit}>
              <FormFieldset id="publicationForm" legend={stepHeading}>
                <p>Search or select a theme to find publications</p>
                <FormGroup className="govuk-!-margin-bottom-3">
                  <PrototypeFormTextSearchInput
                    id={`${formId}-publicationIdSearch`}
                    label="Search publications"
                    name="publicationSearch"
                    onChange={event => {
                      setSearchTerm(event.target.value);
                      setSelectedThemeId('');
                    }}
                    onKeyPress={event => {
                      if (event.key === 'Enter') {
                        event.preventDefault();
                      }
                    }}
                    value={searchTerm}
                    width={20}
                  />
                  {searchTerm && publications.length > 0 && (
                    <p>
                      <a
                        href="#publications"
                        className="govuk-link govuk-!-margin-top-3 s"
                      >
                        Skip to search results
                      </a>
                    </p>
                  )}
                </FormGroup>

                <p>or</p>

                <div className={styles.optionsContainer}>
                  <FormRadioGroup
                    id={`${formId}-themes`}
                    legend="Select a theme"
                    legendSize="s"
                    name="themeId"
                    small
                    options={options.map(option => {
                      return {
                        label: option.title,
                        value: option.id,
                      };
                    })}
                    order={[]}
                    value={selectedThemeId}
                    onChange={e => {
                      setSelectedThemeId(e.target.value);
                    }}
                  />

                  <div className={styles.publicationsList} id="publications">
                    <RHFFormFieldRadioGroup<PublicationFormValues>
                      id={`${formId}-publications`}
                      legend={
                        <>
                          Select a publication
                          <span
                            className="govuk-visually-hidden"
                            aria-live="polite"
                            aria-atomic="true"
                          >
                            {' '}
                            {`${publications.length} results`}
                          </span>
                        </>
                      }
                      legendSize="s"
                      name="publicationId"
                      small
                      options={orderBy(publications, 'title').map(
                        publication => {
                          return {
                            label: publication.title,
                            value: publication.id,
                          };
                        },
                      )}
                    />

                    {!publications.length && (
                      <p>Search or select a theme to view publications</p>
                    )}

                    <Button className="govuk-!-margin-top-4" type="submit">
                      Next step
                    </Button>
                  </div>
                </div>
              </FormFieldset>
            </RHFForm>
          );
        }

        const publication = options
          .flatMap(option => option.topics)
          .flatMap(option => option.publications)
          .find(
            option =>
              option.id === (values as PublicationFormValues).publicationId,
          );

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
    </FormProvider>
  );
};

export default PrototypePublicationForm;
