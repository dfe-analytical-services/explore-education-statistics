import { useMobileMedia } from '@common/hooks/useMedia';
import { Dictionary } from '@common/types';
import styles from '@frontend/modules/data-catalogue/components/DataSetFilePageNav.module.scss';
import classNames from 'classnames';
import React from 'react';

interface Props<TSections extends Dictionary<string>> {
  activeSection: string;
  sections: TSections;
  onClickItem: (id: keyof TSections) => void;
}

export default function DataSetFilePageNav<
  TSections extends Dictionary<string>,
>({ activeSection, sections, onClickItem }: Props<TSections>) {
  const { isMedia: isMobileMedia } = useMobileMedia();

  return (
    <div
      className={classNames('govuk-grid-column-one-third', styles.navContainer)}
    >
      <h2 className="govuk-heading-s" id="nav-heading">
        On this page
      </h2>
      <nav aria-labelledby="nav-heading" role="navigation">
        <ul className={classNames('govuk-list', styles.navList)}>
          {Object.keys(sections).map(id => (
            <NavItem
              hashId={id}
              key={id}
              text={sections[id]}
              isActive={!isMobileMedia && activeSection === id}
              onClick={() => onClickItem(id)}
            />
          ))}

          {isMobileMedia ? (
            <hr />
          ) : (
            <NavItem
              className="govuk-!-padding-top-4"
              hashId="main-content"
              text="Back to top"
              onClick={() => onClickItem('dataSetDetails')}
            />
          )}
        </ul>
      </nav>
    </div>
  );
}

interface NavItemProps {
  className?: string;
  hashId: string;
  isActive?: boolean;
  text: string;
  onClick: () => void;
}

function NavItem({
  className,
  hashId,
  isActive = false,
  text,
  onClick,
}: NavItemProps) {
  return (
    <li className={className}>
      <a
        className={classNames(
          'govuk-link--no-visited-state govuk-link--no-underline',
          {
            'govuk-!-font-weight-bold': isActive,
          },
        )}
        href={`#${hashId}`}
        onClick={onClick}
      >
        {text}
      </a>
    </li>
  );
}
