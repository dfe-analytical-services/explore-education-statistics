import React, { useState } from 'react';
import Accordion from '@common/components/Accordion';
import AccordionSection from '@common/components/AccordionSection';
import Button from '@common/components/Button';
import ButtonGroup from '@common/components/ButtonGroup';
import ButtonText from '@common/components/ButtonText';
import Details from '@common/components/Details';
import SummaryList from '@common/components/SummaryList';
import SummaryListItem from '@common/components/SummaryListItem';
import WarningMessage from '@common/components/WarningMessage';
import { FormGroup, FormTextInput } from '@common/components/form';
import FormFileInput from '@common/components/form/FormFileInput';

const PrototypeMetaPreview = () => {
  const [originalData, setOriginalData] = useState(true);
  const [updatedData, setUpdatedData] = useState(false);
  const [replaceData, setReplaceData] = useState(false);
  const [replaceInProgress, setReplaceInProgress] = useState(false);
  return (
    <div>
      <div className="govuk-inset-text">
        <h2 className="govuk-heading-m">Before you start</h2>
        <div className="govuk-list--bullet">
          <li>
            make sure your data files have passed the checks in our{' '}
            <a href="https://rsconnect/rsc/dfe-published-data-qa/">
              screening app
            </a>
          </li>
          <li>
            if your data doesn’t meet these standards, you won’t be able to
            upload it to your release
          </li>
          <li>
            if you have any issues uploading data and files, or questions about
            data standards contact:{' '}
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
        <FormFileInput id="dataFile" name="dataFile" label="Upload data file" />
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
                    information below. Making this change could affect existing
                    datablocks and footnotes.
                  </WarningMessage>

                  <h2 className="govuk-heading-m">
                    <span className="govuk-tag govuk-tag--green">
                      Data blocks: All OK
                    </span>
                  </h2>
                  <p>All data blocks are still valid, no action is required</p>

                  <Details
                    summary="Example datablock 1: OK"
                    className="govuk-!-margin-bottom-0"
                  >
                    <SummaryList className="govuk-!-margin-bottom-6">
                      <SummaryListItem term="Filter items">
                        Male
                      </SummaryListItem>
                      <SummaryListItem term="Indicator">
                        Number of pupil enrolements
                      </SummaryListItem>
                      <SummaryListItem term="Location">
                        Local authority, West Minster
                      </SummaryListItem>
                      <SummaryListItem term="Time periods">
                        Academic year 2016/17
                      </SummaryListItem>
                    </SummaryList>
                  </Details>

                  <Details summary="Example datablock 2: OK">
                    <SummaryList className="govuk-!-margin-bottom-6">
                      <SummaryListItem term="Filter items">
                        Female
                      </SummaryListItem>
                      <SummaryListItem term="Indicator">
                        Number of pupil enrolements
                      </SummaryListItem>
                      <SummaryListItem term="Location">
                        Local authority, West Minster
                      </SummaryListItem>
                      <SummaryListItem term="Time periods">
                        Academic year 2016/17
                      </SummaryListItem>
                    </SummaryList>
                  </Details>

                  <h2 className="govuk-heading-m">
                    <span className="govuk-tag govuk-tag--green">
                      Footnotes: All OK
                    </span>
                  </h2>
                  <p>All footnotes are still valid, no action is required</p>
                  <Details
                    summary="Example footnote 1: OK"
                    className="govuk-!-margin-bottom-0"
                  >
                    <SummaryList className="govuk-!-margin-bottom-6">
                      <SummaryListItem term="Filters">
                        Characteristics
                      </SummaryListItem>
                      <SummaryListItem term="Filter groups">
                        Gender
                      </SummaryListItem>
                      <SummaryListItem term="Filter items">
                        Male
                      </SummaryListItem>
                      <SummaryListItem term="Indicators">
                        Number of pupil enrolements
                      </SummaryListItem>
                    </SummaryList>
                  </Details>
                  <Details
                    summary="Example footnote 2: OK"
                    className="govuk-!-margin-bottom-0"
                  >
                    <SummaryList className="govuk-!-margin-bottom-6">
                      <SummaryListItem term="Filters">
                        Characteristics
                      </SummaryListItem>
                      <SummaryListItem term="Filter groups">
                        Gender
                      </SummaryListItem>
                      <SummaryListItem term="Filter items">
                        Female
                      </SummaryListItem>
                      <SummaryListItem term="Indicators">
                        Number of pupil enrolements
                      </SummaryListItem>
                    </SummaryList>
                  </Details>

                  <hr className="govuk-section-break govuk-section-break--xl govuk-section-break--visible" />

                  <h2 className="govuk-heading-m">
                    <span className="govuk-tag govuk-tag--red">
                      Data blocks: Warning
                    </span>
                  </h2>
                  <p>
                    One or more data blocks will be invalidated by this data
                    replacement. The list below shows the affected data blocks,
                    you can either delete or edit these if you wish to continue
                    with this data replacement.{' '}
                  </p>

                  <Details
                    summary="Example datablock 1: OK"
                    className="govuk-!-margin-bottom-0"
                  >
                    <SummaryList className="govuk-!-margin-bottom-6">
                      <SummaryListItem term="Filter items">
                        Male
                      </SummaryListItem>
                      <SummaryListItem term="Indicator">
                        Number of pupil enrolements
                      </SummaryListItem>
                      <SummaryListItem term="Location">
                        Local authority, West Minster
                      </SummaryListItem>
                      <SummaryListItem term="Time periods">
                        Academic year 2016/17
                      </SummaryListItem>
                    </SummaryList>
                  </Details>

                  <Details summary="Example datablock 2: WARNING">
                    <SummaryList className="govuk-!-margin-bottom-6">
                      <SummaryListItem term="Filter items">
                        Female
                      </SummaryListItem>
                      <SummaryListItem term="Indicator">
                        Number of pupil enrolements <br />
                        <span className="govuk-tag govuk-tag--red">
                          Not present
                        </span>
                      </SummaryListItem>
                      <SummaryListItem term="Location">
                        Local authority, West Minster
                      </SummaryListItem>
                      <SummaryListItem term="Time periods">
                        Academic year 2016/17
                      </SummaryListItem>
                      <SummaryListItem term="Actions">
                        <Button className="govuk-!-margin-right-3">
                          Delete
                        </Button>
                        <Button className="govuk-button--secondary">
                          Edit
                        </Button>
                      </SummaryListItem>
                    </SummaryList>
                  </Details>

                  <h2 className="govuk-heading-m">
                    <span className="govuk-tag govuk-tag--red">
                      Footnotes: Warning
                    </span>
                  </h2>
                  <p>
                    One or more footnotes will be invalidated by this data
                    replacement. The list below shows the affected footnotes,
                    you can either delete or edit these if you wish to continue
                    with this data replacement.{' '}
                  </p>
                  <Details
                    summary="Example footnote 1: OK"
                    className="govuk-!-margin-bottom-0"
                  >
                    <SummaryList className="govuk-!-margin-bottom-6">
                      <SummaryListItem term="Filters">
                        Characteristics
                      </SummaryListItem>
                      <SummaryListItem term="Filter groups">
                        Gender
                      </SummaryListItem>
                      <SummaryListItem term="Filter items">
                        Male
                      </SummaryListItem>
                      <SummaryListItem term="Indicators">
                        Number of pupil enrolements
                      </SummaryListItem>
                    </SummaryList>
                  </Details>
                  <Details
                    summary="Example footnote 2: WARNING"
                    className="govuk-!-margin-bottom-0"
                  >
                    <SummaryList className="govuk-!-margin-bottom-6">
                      <SummaryListItem term="Filters">
                        Characteristics <br />
                        <span className="govuk-tag govuk-tag--red">
                          Not present
                        </span>
                      </SummaryListItem>
                      <SummaryListItem term="Filter groups">
                        Gender
                      </SummaryListItem>
                      <SummaryListItem term="Filter items">
                        Male
                      </SummaryListItem>
                      <SummaryListItem term="Indicators">
                        Number of pupil enrolements <br />
                        <span className="govuk-tag govuk-tag--red">
                          Not present
                        </span>
                      </SummaryListItem>
                      <SummaryListItem term="Actions">
                        <Button className="govuk-!-margin-right-3">
                          Delete
                        </Button>
                        <Button className="govuk-button--secondary">
                          Edit
                        </Button>
                      </SummaryListItem>
                    </SummaryList>
                  </Details>

                  <hr className="govuk-section-break govuk-section-break--xl govuk-section-break--visible" />

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
                  {!replaceInProgress && <ButtonText>Delete files</ButtonText>}
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
  );
};

export default PrototypeMetaPreview;
