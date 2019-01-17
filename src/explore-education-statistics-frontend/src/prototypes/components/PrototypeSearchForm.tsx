import React from 'react';
import styles from './PrototypeSearchForm.module.scss';

const PrototypeSearchForm = () => (
  <form className={styles.container}>
    <div className="govuk-form-group govuk-!-margin-bottom-0">
      <label className="govuk-label" htmlFor="search">
        Find any DfE statistic, publication or indicator
      </label>

      <input
        className="govuk-input govuk-!-width-three-quarters"
        id="search"
        type="search"
      />
      <button type="submit" className="govuk-button govuk-!-margin-bottom-0">
        Search
      </button>
    </div>
  </form>
);

export default PrototypeSearchForm;
