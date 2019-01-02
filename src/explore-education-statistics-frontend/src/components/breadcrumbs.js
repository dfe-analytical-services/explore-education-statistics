import React, { Component } from 'react';
import { Link } from 'react-router-dom'
import PropTypes from 'prop-types'

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
                    <li className="govuk-breadcrumbs__list-item"  aria-current="page">{this.props.current}</li>
                </ol>
            </div>
        );
    }
}

Breadcrumbs.propTypes = {
    current: PropTypes.string
}

Breadcrumbs.defaultProps = {
    current: ''
}

export default Breadcrumbs;