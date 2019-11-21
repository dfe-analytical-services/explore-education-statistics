import React, { useContext, useEffect, useState } from 'react';
import methodologyService from '@admin/services/methodology/service';
import Page from '@admin/components/Page';
import { IdTitlePair } from '@admin/services/common/types';

interface Model {
  methodologies: IdTitlePair[];
}

const BauMethodologyPage = () => {
  const [model, setModel] = useState<Model>();

  useEffect(() => {
    methodologyService.getMethodologies().then(methodologies => {
      setModel({
        methodologies,
      });
    });
  }, []);

  return (
    <Page>
      <h1>BAU Methodology</h1>

      {model && (
        <table className="govuk-table">
          <caption className="govuk-table__caption">Methodologies list</caption>
          <thead className="govuk-table__head">
            <tr className="govuk-table__row">
              <th scope="col" className="govuk-table__header">
                Methodology
              </th>
              <th scope="col" className="govuk-table__header">
                Status
              </th>
              <th scope="col" className="govuk-table__header">
                Publications
              </th>
            </tr>
          </thead>
          <tbody className="govuk-table__body">
            {model.methodologies.map(methodology => (
              <tr className="govuk-table__row" key={methodology.id}>
                <th scope="row" className="govuk-table__header">
                  {methodology.title}
                </th>
                <td className="govuk-table__cell">
                  <strong className="govuk-tag">Draft</strong>
                </td>
                <td className="govuk-table__cell">
                  <ul className="govuk-list">
                    <li>Pupil absence in schools in England</li>
                    <li>Pupil absence in schools in England (Spring)</li>
                  </ul>
                </td>
              </tr>
            ))}
          </tbody>
        </table>
      )}
    </Page>
  );
};

export default BauMethodologyPage;
