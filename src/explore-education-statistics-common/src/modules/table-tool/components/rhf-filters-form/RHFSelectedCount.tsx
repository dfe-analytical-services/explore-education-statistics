import Tag from '@common/components/Tag';
import React from 'react';
import { Path, Control, useWatch } from 'react-hook-form';

type RHFSelectedCountProps<TFormValues> = {
  control: Control<TFormValues>;
  name: Path<TFormValues>;
};

const RHFSelectedCount = <TFormValues extends Record<string, unknown>>({
  control,
  name,
}: RHFSelectedCountProps<TFormValues>) => {
  const values = useWatch({ control, name }) as string[];
  const count = values ? values.length : 0;

  return count > 0 ? (
    <Tag className="govuk-!-margin-left-2 govuk-!-font-size-14">
      <span className="govuk-visually-hidden"> - </span>
      {count} selected
    </Tag>
  ) : null;
};

export default RHFSelectedCount;
