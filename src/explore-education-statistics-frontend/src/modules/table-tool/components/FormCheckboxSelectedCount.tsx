import classNames from 'classnames';
import { connect, FormikContext, FormikValues } from 'formik';
import get from 'lodash/get';
import React from 'react';
import styles from './FormCheckboxSelectedCount.module.scss';

interface Props {
  name: string;
}

const FormCheckboxSelectedCount = ({
  formik,
  name,
}: Props & { formik: FormikContext<FormikValues> }) => {
  const value = get(formik.values, name);
  let count = 0;

  if (Array.isArray(value)) {
    count = value.length;
  }

  return (
    <>
      {count > 0 ? (
        <span className={classNames('govuk-tag', styles.selectionCount)}>
          {count} selected
        </span>
      ) : null}
    </>
  );
};

export default connect<Props, {}>(FormCheckboxSelectedCount);
