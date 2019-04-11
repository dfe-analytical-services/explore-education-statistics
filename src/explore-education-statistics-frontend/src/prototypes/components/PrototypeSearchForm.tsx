import React from 'react';
import styles from './PrototypeSearchForm.module.scss';

const PrototypeSearchForm = () => (
  <form className={styles.container}>
    <div className="govuk-form-group govuk-!-margin-bottom-0">
      <label className="govuk-label govuk-visually-hidden" htmlFor="search">
        Find on this page
      </label>

      <input
        className="govuk-input"
        id="search"
        placeholder="Search this page"
        type="search"
      />
      <input
        type="submit"
        className={styles.dfeSearchButton}
        value="Search this page"
      />
    </div>
  </form>
);

export default PrototypeSearchForm;
