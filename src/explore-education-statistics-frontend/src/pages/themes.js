import React, { Component } from 'react';
import axios from 'axios';
import DataList from '../components/datalist';
import Title from '../components/title';

class Themes extends Component {

    constructor(props) {
        super(props)
        this.state = {
            data: []
        }
    }

    componentDidMount() {
        axios.get('http://localhost:5010/api/Theme')
            .then(json => this.setState({ data: json.data }))
            .catch(error => alert(error))
    }

    render() {
        const { data } = this.state;
        return (
            <div className="govuk-grid-row">
                <div className="govuk-grid-column-two-thirds">
                    <Title label='Themes' />
                    <DataList data={data} linkIdentifier='theme' />
                </div>
            </div>
        );
    }
}

export default Themes;