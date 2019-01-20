import GovUkTabs from 'govuk-frontend/components/tabs/tabs';
import React, { Component, createRef, ReactElement, ReactNode } from 'react';
import isComponentType from '../lib/type-guards/components/isComponentType';
import TabsSection, { TabsSectionProps } from './TabsSection';

interface Props {
  children: ReactNode;
}

class Tabs extends Component<Props> {
  private ref = createRef<HTMLDivElement>();

  public componentDidMount(): void {
    const tabs = new GovUkTabs(this.ref.current);
    tabs.init();
  }

  public render() {
    const { children } = this.props;
    let sections: ReactElement<TabsSectionProps>[] = [];

    if (Array.isArray(children)) {
      sections = children.filter(child => {
        return isComponentType(child, TabsSection);
      }) as ReactElement<TabsSectionProps>[];
    } else if (isComponentType(children, TabsSection)) {
      sections = [children];
    }

    return (
      <div className="govuk-tabs" ref={this.ref}>
        {this.renderTabsList(sections)}
        {sections}
      </div>
    );
  }

  private renderTabsList(sections: ReactElement<TabsSectionProps>[]) {
    return (
      <ul className="govuk-tabs__list">
        {sections.map(({ props }) => (
          <li className="govuk-tabs__list-item" key={props.id}>
            <a
              className="govuk-tabs__tab govuk-tabs__tab--selected"
              href={`#${props.id}`}
            >
              {props.title}
            </a>
          </li>
        ))}
      </ul>
    );
  }
}

export default Tabs;
