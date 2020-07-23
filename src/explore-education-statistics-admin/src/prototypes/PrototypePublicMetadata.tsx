import PrototypePage from '@admin/prototypes/components/PrototypePage';
import React from 'react';
import CreateMeta from './components/PrototypeMetaCreate';

const PrototypeExamplePage = () => {
  return (
    <PrototypePage
      breadcrumbs={[
        { name: 'Find statistics and data', link: '#' },
        { name: 'An example publication', link: '#' },
        { name: 'Meta guidance document', link: '#' },
      ]}
    >
      <CreateMeta publicView />
    </PrototypePage>
  );
};

export default PrototypeExamplePage;
