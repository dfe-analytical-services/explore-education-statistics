import React, { Component } from 'react';
import { Link } from 'react-router-dom'

class Breadcrumbs extends Component {
    render() {
        return (
            <div className="govuk-breadcrumbs">
                <ol className="govuk-breadcrumbs__list">
                    <li className="govuk-breadcrumbs__list-item">
                        <Link className="govuk-breadcrumbs__link" to='/'>Home</Link>
                    </li>
                    <li className="govuk-breadcrumbs__list-item">
                        <Link className="govuk-breadcrumbs__link" to='/'>Education training and skills</Link>
                    </li>
                    <li className="govuk-breadcrumbs__list-item">
                    <Link className="govuk-breadcrumbs__link" to='/themes'>Themes</Link>
                    </li>
                    <Link className="govuk-breadcrumbs__list-item" to='/themes' aria-current="page">Schools</Link>
                </ol>
            </div>
        );
    }
}

export default Breadcrumbs;