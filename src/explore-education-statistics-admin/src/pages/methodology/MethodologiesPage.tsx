import Link from '@admin/components/Link';
import Page from '@admin/components/Page';
import methodologyRoutes from '@admin/routes/edit-methodology/routes';
import methodologyService from '@admin/services/methodology/service';
import { MethodologyStatus } from '@admin/services/methodology/types';
import RelatedInformation from '@common/components/RelatedInformation';
import Tabs from '@common/components/Tabs';
import TabsSection from '@common/components/TabsSection';
import React, { useEffect, useState } from 'react';

interface Model {
  liveMethodologies: MethodologyStatus[];
  otherMethodologies: MethodologyStatus[];
}

const MethodologiesPage = () => {
  const [model, setModel] = useState<Model>();

  useEffect(() => {
    methodologyService.getMethodologies().then(methodologies => {
      const liveMethodologies: MethodologyStatus[] = [];
      setModel({
        otherMethodologies: methodologies.filter(method => {
          if (method.status.toLocaleLowerCase() === 'live') {
            liveMethodologies.push(method);
            return false;
          }
          return true;
        }),
        liveMethodologies,
      });
    });
  }, []);

  return (
    <Page wide breadcrumbs={[{ name: 'Manage methodologies' }]}>
      <div className="govuk-grid-row">
        <div className="govuk-grid-column-two-thirds">
          <h1 className="govuk-heading-xl">Manage methodologies</h1>
        </div>
        <div className="govuk-grid-column-one-third">
          <RelatedInformation heading="Help and guidance">
            <ul className="govuk-list">
              <li>
                <Link to="/documentation" target="blank">
                  Creating new methodology{' '}
                </Link>
              </li>
            </ul>
          </RelatedInformation>
        </div>
      </div>

      <Tabs id="methodologyTabs">
        <TabsSection id="manage-methodology" title="Methodologies">
          {model && model.liveMethodologies.length ? (
            <table className="govuk-table">
              <thead className="govuk-table__head">
                <tr className="govuk-table__row">
                  <th scope="col" className="govuk-table__header">
                    Methodology title
                  </th>
                  <th scope="col" className="govuk-table__header">
                    Status
                  </th>
                  <th scope="col" className="govuk-table__header">
                    Related publications
                  </th>
                </tr>
              </thead>
              <tbody className="govuk-table__body">
                {model.liveMethodologies.map(methodology => (
                  <tr className="govuk-table__row" key={methodology.id}>
                    <td className="govuk-table__header">
                      <Link
                        to={methodologyRoutes[0].generateLink(methodology.id)}
                      >
                        {methodology.title}
                      </Link>
                    </td>
                    <td className="govuk-table__cell">
                      <strong className="govuk-tag">
                        {methodology.status}
                      </strong>
                    </td>
                    <td className="govuk-table__cell">
                      <ul className="govuk-list">
                        {methodology.publications.map(publication => (
                          <li key={publication.id}>{publication.title}</li>
                        ))}
                      </ul>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          ) : (
            <div className="govuk-inset-text">
              There is currently no draft methodology
            </div>
          )}
          <Link to="/methodologies/create" className="govuk-button">
            Create new methodology
          </Link>
        </TabsSection>
        <TabsSection
          id="draft-methodology"
          title={`Draft methodologies ${
            model && model.otherMethodologies.length
              ? `(${model.otherMethodologies.length})`
              : '(0)'
          }`}
        >
          {model && model.otherMethodologies.length ? (
            <table className="govuk-table">
              <thead className="govuk-table__head">
                <tr className="govuk-table__row">
                  <th scope="col" className="govuk-table__header">
                    Methodology title
                  </th>
                  <th scope="col" className="govuk-table__header">
                    Status
                  </th>
                  <th scope="col" className="govuk-table__header">
                    Related publications
                  </th>
                </tr>
              </thead>
              <tbody className="govuk-table__body">
                {model.otherMethodologies.map(methodology => (
                  <tr className="govuk-table__row" key={methodology.id}>
                    <td className="govuk-table__header">
                      <Link
                        to={methodologyRoutes[0].generateLink(methodology.id)}
                      >
                        {methodology.title}
                      </Link>
                    </td>
                    <td className="govuk-table__cell">
                      <strong className="govuk-tag">
                        {methodology.status}
                      </strong>
                    </td>
                    <td className="govuk-table__cell">
                      <ul className="govuk-list">
                        {methodology.publications.map(publication => (
                          <li key={publication.id}>{publication.title}</li>
                        ))}
                      </ul>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          ) : (
            <div className="govuk-inset-text">
              There is currently no draft methodology
            </div>
          )}
        </TabsSection>
      </Tabs>
    </Page>
  );
};

export default MethodologiesPage;
