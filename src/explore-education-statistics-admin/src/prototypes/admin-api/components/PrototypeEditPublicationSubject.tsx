import ButtonText from '@common/components/ButtonText';
import { Form, FormFieldSelect } from '@common/components/form';
import Button from '@common/components/Button';
import Yup from '@common/validation/yup';
import React from 'react';
import { Formik } from 'formik';
import {
  PrototypeSubject,
  PublicationSubject,
} from '../PrototypePublicationSubjects';

interface FormValues {
  subjectId: string;
}

interface Props {
  publicationSubject: PublicationSubject;
  subjects: PrototypeSubject[];
  onClose: () => void;
  onSubmit: (updatedPublicationSubject: PublicationSubject) => void;
}

const PrototypeEditPublicationSubject = ({
  publicationSubject,
  subjects,
  onClose,
  onSubmit,
}: Props) => {
  return (
    <>
      <ButtonText
        className="govuk-!-margin-bottom-6 govuk-!-padding-left-3 govuk-link govuk-back-link"
        onClick={onClose}
      >
        Back to API data sets
      </ButtonText>

      <section>
        <span className="govuk-caption-l">{publicationSubject.title}</span>
        <h2>Change data set to publish</h2>

        <Formik<FormValues>
          initialValues={{
            subjectId: publicationSubject.subjectId,
          }}
          validationSchema={Yup.object({
            subjectId: Yup.string().required('Enter a subject'),
          })}
          onSubmit={values => {
            onSubmit({
              ...publicationSubject,
              subjectId: values.subjectId,
            });
          }}
        >
          {() => (
            <Form id="form">
              <FormFieldSelect<FormValues>
                id="subjectId"
                name="subjectId"
                label="Available data sets"
                options={subjects.map(s => ({
                  label: s.title,
                  value: s.id,
                }))}
                placeholder="Choose a subject"
              />
              <Button type="submit">Save</Button>
            </Form>
          )}
        </Formik>
      </section>
    </>
  );
};

export default PrototypeEditPublicationSubject;
