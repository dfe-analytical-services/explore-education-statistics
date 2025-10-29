import Tag from '@common/components/Tag';
import { useWatch } from 'react-hook-form';
import React, { memo } from 'react';

interface Props {
  name: string;
}

const FormCheckboxSelectedCount = ({ name }: Props) => {
  const value = useWatch({ name });
  let count = 0;

  if (Array.isArray(value)) {
    count = value.length;
  }

  return count > 0 ? (
    <Tag
      className="govuk-!-margin-left-2 govuk-!-font-size-16"
      testId={`${name}-count`}
    >
      <span className="govuk-visually-hidden"> - </span>
      {count} selected
    </Tag>
  ) : null;
};

export default memo(FormCheckboxSelectedCount);
