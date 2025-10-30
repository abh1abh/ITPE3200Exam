import React from "react";
import { Carousel, Image } from "react-bootstrap";
const API_URL = import.meta.env.VITE_API_URL;
console.log(API_URL);

const HomePage: React.FC = () => {
  return (
    <div className="text-center">
      <h1 className="display-4">Welcome to My Shop</h1>
      <Carousel>
        <Carousel.Item>
          <Image
            src={`${API_URL}/images/man_happy.jpg`}
            className="d-block w-100"
            alt="Happy man"
            // style={{ maxHeight: "500px", objectFit: "cover" }}
          />
        </Carousel.Item>
        <Carousel.Item>
          <Image
            src={`${API_URL}/images/man_with_nurse.jpg`}
            className="d-block w-100"
            alt="Man with nurse"
            // style={{ maxHeight: "500px", objectFit: "cover" }}
          />
        </Carousel.Item>
        <Carousel.Item>
          <Image
            src={`${API_URL}/images/doctor.jpg`}
            className="d-block w-100"
            alt="Doctor"
            // style={{ maxHeight: "500px", objectFit: "cover" }}
          />
        </Carousel.Item>
      </Carousel>
    </div>
  );
};

export default HomePage;
