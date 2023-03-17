import Button from '@common/components/Button';
import {
  Form,
  FormFieldSelect,
  // FormFieldTextInput,
} from '@common/components/form';
import InsetText from '@common/components/InsetText';
import Yup from '@common/validation/yup';
import { Formik } from 'formik';
import React from 'react';
import {
  PublicationSubject,
  PrototypeSubject,
} from '../PrototypePublicationSubjects';

interface FormValues {
  title: string;
  subjectId: string;
}

interface Props {
  subjects: PrototypeSubject[];
  isCurrentReleasePublished?: boolean;
  onSubmit: (subject: PublicationSubject) => void;
}

const PrototypeAddPublicationSubject = ({
  subjects,
  isCurrentReleasePublished,
  onSubmit,
}: Props) => {
  return (
    <>
      <h2>API datasets</h2>
      <InsetText>
        <h3>Before you start</h3>
        <p>
          API datasets are available for use in the EES public API for
          third-party applications to consume. They represent a long-lived data
          series where the data structure cannot change in a drastic way between
          each release.
        </p>
        <p>Please note the following constraints to using API datasets:</p>
        <ul>
          <li>
            they <strong>cannot</strong> be deleted once published
          </li>
          <li>
            their facets (column variables) <strong>cannot</strong> be deleted
            once published
          </li>
          <li>
            existing facets (column variables) must always map to equivalent
            facets when linking to new subject data
          </li>
        </ul>
        <p>
          If your dataset cannot meet these criteria, we advise you to continue
          using standard datasets, but note these will not be available in the
          public api.
        </p>
      </InsetText>

      {!isCurrentReleasePublished && (
        <Formik<FormValues>
          initialValues={{
            title: '',
            subjectId: '',
          }}
          validationSchema={Yup.object({
            subjectId: Yup.string().required('Select a dataset'),
            // title: Yup.string().required('Enter a title'),
          })}
          onSubmit={(values, { resetForm }) => {
            resetForm();
            onSubmit(values);
          }}
        >
          {() => (
            <Form id="form" showSubmitError>
              {/* 
                <FormFieldTextInput
                  id="title"
                  name="title"
                  label="Title"
                  width={20}
                /> 
              */}
              <FormFieldSelect<FormValues>
                id="subjectId"
                name="subjectId"
                label="Available datasets"
                options={subjects.map(s => ({
                  label: s.title,
                  value: s.id,
                }))}
                placeholder="Select a dataset"
              />
              <Button type="submit">Create API dataset</Button>
            </Form>
          )}
        </Formik>
      )}
    </>
  );
};

export default PrototypeAddPublicationSubject;
