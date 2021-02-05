import classNames from 'classnames';
import React from 'react';
import Details from '@common/components/Details';
import PrototypeDownloadAncillaryLinks from './PrototypeDownloadAncillaryLinks';
import styles from '../PrototypePublicPage.module.scss';

interface Props {
  viewAsList?: boolean;
}

const PrototypeDownloadAncillary = ({ viewAsList }: Props) => {
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
