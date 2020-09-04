import PageTitle from '@admin/components/PageTitle';
import React, { useState } from 'react';
import Accordion from '@common/components/Accordion';
import AccordionSection from '@common/components/AccordionSection';
import Button from '@common/components/Button';
import ButtonGroup from '@common/components/ButtonGroup';
import ButtonText from '@common/components/ButtonText';
import SanitizeHtml from '@common/components/SanitizeHtml';
import SummaryList from '@common/components/SummaryList';
import SummaryListItem from '@common/components/SummaryListItem';
import WarningMessage from '@common/components/WarningMessage';
import {
  Form,
  FormFieldset,
  FormGroup,
  FormTextInput,
} from '@common/components/form';
import FormFieldFileInput from '@common/components/form/FormFieldFileInput';
import FormFileInput, {
  FormFileInputProps,
} from '@common/components/form/FormFileInput';
import FormFieldTextInput from '@common/components/form/FormFieldTextInput';
import MetaVariables from './PrototypeMetaVariables';

const PrototypeMetaPreview = () => {
  const [originalData, setOriginalData] = useState(true);
  const [updatedData, setUpdatedData] = useState(false);
  const [replaceData, setReplaceData] = useState(false);
  const [replaceInProgress, setReplaceInProgress] = useState(false);
  return (
    <>
      <div>
        <div className="govuk-inset-text">
          <h2 className="govuk-heading-m">Before you start</h2>
          <div className="govuk-list--bullet">
            <li>
              make sure your data has passed the screening checks in our{' '}
              <a href="https://github.com/dfe-analytical-services/ees-data-screener">
                R Project
              </a>{' '}
            </li>
            <li>
              if your data doesn’t meet these standards, you won’t be able to
              upload it to your release
            </li>
            <li>
              if you have any issues uploading data and files, or questions
              about data standards contact:{' '}
              <a href="mailto:explore.statistics@education.gov.uk">
                explore.statistics@education.gov.uk
              </a>
            </li>
          </div>
        </div>
        <FormGroup>
          <FormTextInput
            id="subjectTitle"
            name="subjectTitle"
            label="Subject title"
            width={20}
          />
        </FormGroup>
        <FormGroup>
          <FormFileInput
            id="dataFile"
            name="dataFile"
            label="Upload data file"
          />
        </FormGroup>
        <FormGroup>
          <FormFileInput
            id="metaDataFile"
            name="metaDataFile"
            label="Upload metadata file"
          />
        </FormGroup>
        <ButtonGroup>
          <Button>Upload data files</Button>
          <ButtonText>Cancel</ButtonText>
        </ButtonGroup>

        <h2 className="govuk-heading-m govuk-!-margin-top-9">Data files</h2>
        <Accordion id="dataFiles">
          <AccordionSection heading="Data example 1" goToTop={false}>
            <SummaryList className="govuk-!-margin-bottom-9">
              <SummaryListItem term="Subject title">
                Data example 1
              </SummaryListItem>
              <SummaryListItem term="Data title">
                example-data-{updatedData ? '2' : '1'}.csv{' '}
                {replaceInProgress && (
                  <>
                    to be replaced by <strong>example-data-2.csv</strong>
                  </>
                )}
              </SummaryListItem>
              <SummaryListItem term="Metadata file">
                example-metadata-{updatedData ? '2' : '1'}.csv{' '}
                {replaceInProgress && (
                  <>
                    to be replaced by <strong>example-metadata-2.csv</strong>
                  </>
                )}
              </SummaryListItem>
              <SummaryListItem term="Data file size">
                {originalData && '15kb'}
                {updatedData && '20kb'}
                {replaceInProgress && (
                  <>
                    Orginal data 15kb, replacement data <strong>20kb</strong>
                  </>
                )}
              </SummaryListItem>
              <SummaryListItem term="Number of rows">
                {originalData && '161'}
                {updatedData && '200'}
                {replaceInProgress && (
                  <>
                    Orginal data 161, replacement data <strong>200</strong>
                  </>
                )}
              </SummaryListItem>

              <SummaryListItem term="Uploaded by">
                <a href="mailto:bau1@example.com">bau1@example.com</a>
              </SummaryListItem>
              <SummaryListItem term="Date uploaded">
                2/9/2020 14:30
              </SummaryListItem>
              <SummaryListItem term="Status">
                {originalData ||
                  (updatedData && <span className="govuk-tag">Complete</span>)}
                {replaceInProgress && (
                  <>
                    <span className="govuk-tag">
                      Data replacement in progress
                    </span>
                    <WarningMessage>
                      Before confirming the data replacement please check the
                      information below. Making this update could affect
                      existing datablocks and footnotes that use this existing
                      data.
                    </WarningMessage>
                    <ul className="govuk-list govuk-list--bullet">
                      <li>
                        <strong className="govuk-tag govuk-tag--green">
                          Footnotes: OK
                        </strong>
                        , still valid no action needed
                      </li>
                      <li>
                        <strong className="govuk-tag govuk-tag--green">
                          Datablocks: OK
                        </strong>
                        , still valid no action needed
                      </li>
                    </ul>
                    <ul className="govuk-list govuk-list--bullet">
                      <li>
                        <strong className="govuk-tag govuk-tag--yellow">
                          Filter or indicator mismatch
                        </strong>{' '}
                        , review as may need amending after the replacement
                      </li>
                      <li>
                        Can we display list of mismatatched filters / indicators
                        here?
                      </li>
                    </ul>
                    <ul className="govuk-list govuk-list--bullet">
                      <li>
                        <strong className="govuk-tag govuk-tag--red">
                          Footnote: Warning
                        </strong>
                        , remove or amend this footnote before confirming the
                        data replacement
                      </li>
                      <li>
                        <strong className="govuk-tag govuk-tag--red">
                          Datablock: Warning
                        </strong>
                        , remove or amend this datablock before confirming the
                        data replacement
                      </li>
                    </ul>
                    <ButtonGroup className="govuk-!-margin-top-9 govuk-!-margin-bottom-9">
                      <Button
                        onClick={() => {
                          setReplaceData(false);
                          setReplaceInProgress(false);
                          setOriginalData(false);
                          setUpdatedData(true);
                        }}
                      >
                        Confirm data replacement
                      </Button>
                    </ButtonGroup>
                  </>
                )}
              </SummaryListItem>
              <SummaryListItem
                term="Actions"
                actions={
                  <>
                    <ButtonText
                      className="govuk-!-margin-right-6"
                      onClick={() => {
                        return (
                          replaceInProgress &&
                            (setOriginalData(true),
                            setReplaceData(false),
                            setReplaceInProgress(false)),
                          (originalData || updatedData) && setReplaceData(true)
                        );
                      }}
                    >
                      {(originalData || updatedData) && 'Replace data files'}
                      {replaceInProgress && 'Revert back to orginal data files'}
                    </ButtonText>{' '}
                    <ButtonText>Delete files</ButtonText>
                  </>
                }
              />
              {replaceData && (
                <SummaryListItem term="Replace data">
                  <FormGroup>
                    <FormFileInput
                      id="replaceDataFile"
                      name="replaceDataFile"
                      label="Upload replacement data file"
                    />
                  </FormGroup>
                  <FormGroup>
                    <FormFileInput
                      id="replacementMetaDataFile"
                      name="replacementMetaDataFile"
                      label="Upload replacement metadata file"
                    />
                  </FormGroup>
                  <ButtonGroup>
                    <Button
                      onClick={() => {
                        setReplaceData(false);
                        setReplaceInProgress(true);
                        setOriginalData(false);
                      }}
                    >
                      Upload replacment data files
                    </Button>
                    <ButtonText
                      onClick={() => {
                        setReplaceData(false);
                        setReplaceInProgress(false);
                        setOriginalData(true);
                      }}
                    >
                      Cancel
                    </ButtonText>
                  </ButtonGroup>
                </SummaryListItem>
              )}
            </SummaryList>
          </AccordionSection>
        </Accordion>
      </div>
    </>
  );
};

export default PrototypeMetaPreview;
