//
//  ParkCell.swift
//  parkapp
//
//  Created by Ilya Sedov on 19/05/2019.
//  Copyright © 2019 SKB Kontur. All rights reserved.
//

import UIKit

class ParkCell: UITableViewCell {
  @IBOutlet weak var titleLabel: UILabel!
  @IBOutlet weak var vacantMarker: UIView!
  @IBOutlet weak var vacantLabel: UILabel!
  @IBOutlet weak var occupiedMarker: UIView!
  @IBOutlet weak var occupiedLabel: UILabel!
  @IBOutlet weak var totalLabel: UILabel!

  var model: Parking!

  override func awakeFromNib() {
    super.awakeFromNib()
    vacantMarker.layer.cornerRadius = vacantMarker.bounds.height/2.0
    occupiedMarker.layer.cornerRadius = occupiedMarker.bounds.height/2.0
  }

  func set(_ model: Parking) {
    self.model = model

    titleLabel.text = model.title
    vacantLabel.text = "\(model.vacant)"
    occupiedLabel.text = "\(model.occupied)"
    totalLabel.text = "Всего: \(model.total)"
  }
}
