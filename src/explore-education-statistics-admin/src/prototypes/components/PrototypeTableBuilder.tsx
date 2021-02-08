import React from 'react';
import styles from '../PrototypePublicPage.module.scss';

const PrototypeDownloadAncillary = () => {
  return (
    <>
      <div className={styles.prototypeDownloadContainer}>
        <div>
          <h3 className="govuk-heading-m">Create your own tables</h3>
          <p>
            Use our tool to build tables using our range of national and
            regional data.
          </p>
        </div>
        <p>
          <a href="#" className="govuk-button  govuk-!-margin-bottom-0">
            Create table
          </a>
        </p>
      </div>
    </>
  );
};

export default PrototypeDownloadAncillary;
