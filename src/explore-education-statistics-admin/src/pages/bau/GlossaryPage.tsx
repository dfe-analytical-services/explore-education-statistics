import Page from '@admin/components/Page';
import React, { useState } from 'react';
import Button from '@common/components/Button';
import glossaryService from '@admin/services/glossaryService';
import styles from './GlossaryPage.module.scss';

const GlossaryPage = () => {
  const [successMessage, setSuccessMessage] = useState('');

  const clearGlossaryCache = async () => {
    setSuccessMessage('Clearing cache...');
    await glossaryService.clearCache();
    setSuccessMessage('Cache cleared successfully');

    setTimeout(() => {
      setSuccessMessage('');
    }, 5000);
  };

  return (
    <Page
      title="Glossary"
      caption="Manage glossary"
      wide
      breadcrumbs={[
        { name: 'Platform administration', link: '/administration' },
        { name: 'Glossary' },
      ]}
    >
      <div className={styles.clearCacheButton}>
        <Button onClick={clearGlossaryCache} className="govuk-!-margin-0">
          Clear glossary cache
        </Button>
        {successMessage && (
          <span className="govuk-!-margin-0">{successMessage}</span>
        )}
      </div>
    </Page>
  );
};

export default GlossaryPage;
