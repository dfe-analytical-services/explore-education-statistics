import releaseDataGuidanceService from '@admin/services/releaseDataGuidanceService';
import FormFieldEditor from '@admin/components/form/FormFieldEditor';
import Accordion from '@common/components/Accordion';
import AccordionSection from '@common/components/AccordionSection';
import Button from '@common/components/Button';
import ButtonGroup from '@common/components/ButtonGroup';
import ButtonText from '@common/components/ButtonText';
import FormFieldTextArea from '@common/components/form/FormFieldTextArea';
import FormProvider from '@common/components/form/FormProvider';
import Form from '@common/components/form/Form';
import InsetText from '@common/components/InsetText';
import LoadingSpinner from '@common/components/LoadingSpinner';
import ContentHtml from '@common/components/ContentHtml';
import WarningMessage from '@common/components/WarningMessage';
import Link from '@admin/components/Link';
import useAsyncHandledRetry from '@common/hooks/useAsyncHandledRetry';
import useToggle from '@common/hooks/useToggle';
import ReleaseDataGuidanceDataFile from '@common/modules/release/components/ReleaseDataGuidanceDataFile';
import minDelay from '@common/utils/minDelay';
import Yup from '@common/validation/yup';
import toPath from 'lodash/toPath';
import React from 'react';

export interface DataGuidanceFormValues {
  content: string;
  dataSets: {
    fileId: string;
    content: string;
  }[];
}

interface Props {
  releaseVersionId: string;
  canUpdateRelease: boolean;
}

const contentMaxLength = 250;
const formId = 'dataGuidanceForm';

const ReleaseDataGuidanceSection = ({
  releaseVersionId,
  canUpdateRelease,
}: Props) => {
  const {
    value: dataGuidance,
    isLoading,
    setState: setDataGuidance,
  } = useAsyncHandledRetry(
    () => releaseDataGuidanceService.getDataGuidance(releaseVersionId),
    [releaseVersionId, canUpdateRelease],
  );

  const [isEditing, toggleEditing] = useToggle(canUpdateRelease);

  const handleSubmit = async (values: DataGuidanceFormValues) => {
    await minDelay(async () => {
      const updatedGuidance =
        await releaseDataGuidanceService.updateDataGuidance(
          releaseVersionId,
          values,
        );

      setDataGuidance({ value: updatedGuidance });

      toggleEditing.off();
    }, 600);
  };

  return (
    <>
      <h2>Public data guidance</h2>

      {canUpdateRelease ? (
        <InsetText>
          <h3>Before you start</h3>

          <ul>
            <li>
              upload at least one data file before creating the data guidance
            </li>
            <li>
              ensure all data guidance has been populated and is up-to-date
              before seeking release approval
            </li>
            <li>
              this document will become public facing when the publication is
              released
            </li>
          </ul>
        </InsetText>
      ) : (
        <WarningMessage>
          This release has been approved, and can no longer be updated.
        </WarningMessage>
      )}

      <LoadingSpinner loading={isLoading}>
        {dataGuidance && (
          <>
            {dataGuidance.dataSets.length > 0 ? (
              <FormProvider
                initialValues={{
                  content: dataGuidance.content,
                  dataSets: dataGuidance.dataSets.map(dataSet => ({
                    fileId: dataSet.fileId,
                    content: dataSet.content,
                  })),
                }}
                validationSchema={Yup.object<DataGuidanceFormValues>({
                  content: Yup.string().required('Enter main guidance content'),
                  dataSets: Yup.array().of(
                    Yup.object({
                      id: Yup.string(),
                      content: Yup.string()
                        .required(params => {
                          const [, index] = toPath(params.path);
                          const dataSet = dataGuidance?.dataSets[Number(index)];

                          if (!dataSet) {
                            return null;
                          }

                          return `Enter file guidance content for ${dataSet.name}`;
                        })
                        .max(
                          contentMaxLength,
                          `File guidance content must be ${contentMaxLength} characters or fewer`,
                        ),
                    }),
                  ),
                })}
              >
                {({ formState, getValues, reset }) => {
                  const hasSubmitValidationError =
                    formState.submitCount > 0 && !formState.isValid
                      ? true
                      : undefined;

                  const values = getValues() as DataGuidanceFormValues;
                  return (
                    <Form id={formId} onSubmit={handleSubmit}>
                      {isEditing ? (
                        <FormFieldEditor<DataGuidanceFormValues>
                          name="content"
                          label="Main guidance content"
                          hint={
                            <>
                              We recommend including individual sections
                              providing a description, information about
                              coverage and details of file formats and
                              conventions. For best practice examples, refer to
                              the{' '}
                              <Link to="https://dfe-analytical-services.github.io/analysts-guide/statistics-production/examples.html#public-data-guidance---overview">
                                data guidance support on the Analyst's Guide
                                (opens in new tab)
                              </Link>
                              .
                            </>
                          }
                        />
                      ) : (
                        <>
                          {!canUpdateRelease && !dataGuidance?.content ? (
                            <InsetText>
                              No guidance content was saved.
                            </InsetText>
                          ) : (
                            <ContentHtml
                              html={values.content}
                              testId="mainGuidanceContent"
                            />
                          )}
                        </>
                      )}

                      {dataGuidance.dataSets.length > 0 && (
                        <>
                          <h3 className="govuk-!-margin-top-6">Data files</h3>

                          <Accordion
                            id="dataGuidance-dataFiles"
                            openAll={hasSubmitValidationError || isEditing}
                          >
                            {dataGuidance.dataSets.map((dataSet, index) => (
                              <AccordionSection
                                heading={dataSet.name}
                                key={dataSet.fileId}
                              >
                                <ReleaseDataGuidanceDataFile
                                  key={dataSet.fileId}
                                  dataSet={dataSet}
                                  renderContent={() =>
                                    isEditing ? (
                                      <FormFieldTextArea<DataGuidanceFormValues>
                                        label="File guidance content"
                                        name={`dataSets.${index}.content`}
                                        rows={3}
                                        maxLength={contentMaxLength}
                                      />
                                    ) : (
                                      <ContentHtml
                                        html={values.dataSets[index].content}
                                        testId="fileGuidanceContent"
                                      />
                                    )
                                  }
                                />
                              </AccordionSection>
                            ))}
                          </Accordion>
                        </>
                      )}

                      {canUpdateRelease && (
                        <ButtonGroup>
                          {isEditing && (
                            <Button
                              type="submit"
                              disabled={formState.isSubmitting}
                            >
                              Save guidance
                            </Button>
                          )}

                          <Button
                            variant={isEditing ? 'secondary' : undefined}
                            onClick={toggleEditing}
                          >
                            {isEditing ? 'Preview guidance' : 'Edit guidance'}
                          </Button>

                          {isEditing && (
                            <ButtonText onClick={() => reset()}>
                              Cancel
                            </ButtonText>
                          )}
                        </ButtonGroup>
                      )}
                    </Form>
                  );
                }}
              </FormProvider>
            ) : (
              <>
                {canUpdateRelease ? (
                  <WarningMessage>
                    Before you can change the public data guidance, you must
                    upload at least one data file.
                  </WarningMessage>
                ) : (
                  <InsetText>
                    The public data guidance has not been created as no data
                    files were uploaded.
                  </InsetText>
                )}
              </>
            )}
          </>
        )}
      </LoadingSpinner>
    </>
  );
};

export default ReleaseDataGuidanceSection;
