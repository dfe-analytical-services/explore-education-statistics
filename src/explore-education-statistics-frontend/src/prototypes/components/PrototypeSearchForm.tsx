import React from 'react';
import styles from './PrototypeSearchForm.module.scss';

const PrototypeSearchForm = () => (
  <form className={styles.container}>
    <div className="govuk-form-group">
      <label className="govuk-label" htmlFor="search">
        Find any DfE statistic, publication or indicator
      </label>

      <input className="govuk-input" id="search" type="search" />
    </div>

    <button type="submit" className="govuk-button govuk-!-margin-bottom-0">
      Search
    </button>
  </form>
);

export default PrototypeSearchForm;
