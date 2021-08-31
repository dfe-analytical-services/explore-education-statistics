import Form from '@common/components/form/Form';
import FormFieldRadioSearchGroup from '@common/components/form/FormFieldRadioSearchGroup';
import { mapFieldErrors } from '@common/validation/serverValidations';
import Button from '@common/components/Button';
import Details from '@common/components/Details';
import FormattedDate from '@common/components/FormattedDate';
import SummaryList from '@common/components/SummaryList';
import SummaryListItem from '@common/components/SummaryListItem';
import ButtonGroup from '@common/components/ButtonGroup';
import ButtonText from '@common/components/ButtonText';
import { BasicMethodology } from '@admin/services/methodologyService';
import Tag from '@common/components/Tag';
import TagGroup from '@common/components/TagGroup';
import useFormSubmit from '@common/hooks/useFormSubmit';
import { RadioOption } from '@common/components/form/FormRadioGroup';
import Yup from '@common/validation/yup';
import { Formik } from 'formik';
import React, { useMemo } from 'react';

interface FormValues {
  methodologyId: string;
}

interface Props {
  methodologies: BasicMethodology[];
  onCancel: () => void;
  onSubmit: (values: FormValues) => void;
}

const AdoptMethodologyForm = ({ methodologies, onCancel, onSubmit }: Props) => {
  const radioOptions = useMemo<RadioOption[]>(
    () =>
      methodologies?.map(methodology => {
        return {
          label: methodology.title,
          value: methodology.id,
          hint: (
            <Details summary="More details" className="govuk-!-margin-bottom-2">
              <SummaryList className="govuk-!-margin-bottom-3">
                <SummaryListItem term="Owning publication">
                  {methodology.owningPublication?.title}
                </SummaryListItem>
                <SummaryListItem term="Status">
                  <TagGroup className="govuk-!-margin-left-2">
                    <Tag>{methodology.status}</Tag>
                    {methodology.amendment && <Tag>Amendment</Tag>}
                  </TagGroup>
                </SummaryListItem>
                <SummaryListItem term="Publish date">
                  {methodology.published ? (
                    <FormattedDate>{methodology.published}</FormattedDate>
                  ) : (
                    'Not yet published'
                  )}
                </SummaryListItem>
              </SummaryList>
            </Details>
          ),
        };
      }),
    [methodologies],
  );

  const errorMappings = [
    mapFieldErrors<FormValues>({
      target: 'methodologyId',
      messages: {
        CANNOT_ADOPT_METHODOLOGY_ALREADY_LINKED_TO_PUBLICATION:
          'Select a methodology that has not already been adopted by this publication',
      },
    }),
  ];

  const handleSubmit = useFormSubmit(async (values: FormValues) => {
    await onSubmit(values);
  }, errorMappings);

  return (
    <Formik<FormValues>
      initialValues={{ methodologyId: '' }}
      onSubmit={handleSubmit}
      validationSchema={Yup.object<FormValues>({
        methodologyId: Yup.string().required('Select a methodology to adopt'),
      })}
    >
      <Form id="adoptMethodologyForm">
        <FormFieldRadioSearchGroup
          id="selectMethodology"
          legend="Select a methodology"
          name="methodologyId"
          options={radioOptions}
        />
        <ButtonGroup>
          <Button type="submit">Save</Button>
          <ButtonText onClick={onCancel}>Cancel</ButtonText>
        </ButtonGroup>
      </Form>
    </Formik>
  );
};

export default AdoptMethodologyForm;
