import Tag from '@common/components/Tag';
import { useFormikContext } from 'formik';
import get from 'lodash/get';
import React from 'react';

interface Props {
  name: string;
}

const FormCheckboxSelectedCount = ({ name }: Props) => {
  const formik = useFormikContext();

  const value = get(formik.values, name);
  let count = 0;

  if (Array.isArray(value)) {
    count = value.length;
  }

  return count > 0 ? (
    <Tag className="govuk-!-margin-left-2 govuk-!-font-size-14">
      <span className="govuk-visually-hidden"> - </span>
      {count} selected
    </Tag>
  ) : null;
};

export default FormCheckboxSelectedCount;
