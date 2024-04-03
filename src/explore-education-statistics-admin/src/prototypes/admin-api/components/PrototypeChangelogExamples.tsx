import React from 'react';
import { Changelog } from '../contexts/PrototypeNextSubjectContext';

const PrototypeChangelogExample = ({ changelog }: { changelog: Changelog }) => {
  const { locations, filters, indicators, versionNotes } = changelog;

  return (
    <>
      <h3>Changelog</h3>

      {versionNotes && (
        <>
          <h4>Version notes</h4>
          <p>{versionNotes}</p>
        </>
      )}

      {locations.newItems.length > 0 && (
        <>
          <h4 className="govuk-!-margin-bottom-0">New locations</h4>

          <ul className="govuk-!-margin-bottom-6">
            {locations.newItems.map(item => (
              <li key={item.id}>{item.label}</li>
            ))}
          </ul>
        </>
      )}

      {locations.mappedItems.length > 0 && (
        <>
          <h4 className="govuk-!-margin-bottom-0">Mapped locations</h4>

          <ul className="govuk-!-margin-bottom-6">
            {locations.mappedItems.map(item => (
              <li key={item[0].id}>
                {item[0].label} <strong>maps to</strong> {item[1].label}
              </li>
            ))}
          </ul>
        </>
      )}

      {locations.noMappingItems.length > 0 && (
        <>
          <h4 className="govuk-!-margin-bottom-0">Unmapped locations</h4>

          <ul className="govuk-!-margin-bottom-6">
            {locations.noMappingItems.map(item => (
              <li key={item.id}>{item.label}</li>
            ))}
          </ul>
        </>
      )}

      {filters.newItems.length > 0 && (
        <>
          <h4 className="govuk-!-margin-bottom-0">New filters</h4>

          <ul className="govuk-!-margin-bottom-6">
            {filters.newItems.map(item => (
              <li key={item.id}>{item.label}</li>
            ))}
          </ul>
        </>
      )}

      {filters.mappedItems.length > 0 && (
        <>
          <h4 className="govuk-!-margin-bottom-0">Mapped filters</h4>

          <ul className="govuk-!-margin-bottom-6">
            {filters.mappedItems.map(item => (
              <li key={item[0].id}>
                {item[0].label} <strong>maps to</strong> {item[1].label}
              </li>
            ))}
          </ul>
        </>
      )}

      {filters.noMappingItems.length > 0 && (
        <>
          <h4 className="govuk-!-margin-bottom-0">Unmapped filters</h4>

          <ul className="govuk-!-margin-bottom-6">
            {filters.noMappingItems.map(item => (
              <li key={item.id}>{item.label}</li>
            ))}
          </ul>
        </>
      )}

      {indicators.newItems.length > 0 && (
        <>
          <h4 className="govuk-!-margin-bottom-0">New indicators</h4>

          <ul className="govuk-!-margin-bottom-6">
            {indicators.newItems.map(item => (
              <li key={item.id}>{item.label}</li>
            ))}
          </ul>
        </>
      )}

      {indicators.mappedItems.length > 0 && (
        <>
          <h4 className="govuk-!-margin-bottom-0">Mapped indicators</h4>

          <ul className="govuk-!-margin-bottom-6">
            {indicators.mappedItems.map(item => (
              <li key={item[0].id}>
                {item[0].label} <strong>maps to</strong> {item[1].label}
              </li>
            ))}
          </ul>
        </>
      )}

      {indicators.noMappingItems.length > 0 && (
        <>
          <h4 className="govuk-!-margin-bottom-0">Unmapped indicators</h4>

          <ul className="govuk-!-margin-bottom-6">
            {indicators.noMappingItems.map(item => (
              <li key={item.id}>{item.label}</li>
            ))}
          </ul>
        </>
      )}
    </>
  );
};

export default PrototypeChangelogExample;
