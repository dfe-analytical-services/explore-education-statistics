import React, { useContext, useEffect, useState } from 'react';
import Page from '@admin/components/Page';
import { MethodologyStatus } from 'src/services/methodology/types';

interface Model {
  methodologies: MethodologyStatus[];
}

const EditMethodologyPage = () => {
  const [model, setModel] = useState<Model>();

  return (
    <Page wide breadcrumbs={[{ name: 'Methodology' }]}>
      <p>placeholder page</p>
    </Page>
  );
};

export default EditMethodologyPage;
